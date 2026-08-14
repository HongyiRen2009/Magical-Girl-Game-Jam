using System;
using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using Unity.Mathematics;

[Serializable]
public class M_Movement : Mod
{
    [SerializeField] bool followPath;

    [AllowNesting] [HideIf("followPath")] [SerializeField] float speed;
    [AllowNesting] [HideIf("followPath")] [SerializeField] float acceleration;

    [AllowNesting] [HideIf("followPath")] [SerializeField] bool tracking;

    bool _tracking => tracking && !followPath;
    [AllowNesting] [ShowIf("_tracking")] [SerializeField] float rotationSpeed;
    [AllowNesting] [ShowIf("_tracking")] [SerializeField] bool pinpointLocation;

    bool _pinpointing => pinpointLocation && !followPath;
    [AllowNesting] [ShowIf("_pinpointing")] [SerializeField] bool trackX = true;
    [AllowNesting] [ShowIf("_pinpointing")] [SerializeField] bool trackY = true;
    [AllowNesting] [ShowIf("_pinpointing")] [SerializeField] float pinpointSpeed;
    [AllowNesting] [ShowIf("_pinpointing")] [SerializeField] float maxPSpeedDistance;

    [AllowNesting] [ShowIf("followPath")] [SerializeField] Vector3 start;
    [AllowNesting] [ShowIf("followPath")] [SerializeField] bool destroyOnFinish = true;

    [Header("Points")] // show if doesnt work in this case so im just making it a seperate catagory
    [SerializeField] Point[] points;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos;

    float elapsed = 0;
    int currentPoint = 0;
    Vector3 startPos;
    

    public override void Begin(Projectile projectile)
    {
        base.Begin(projectile);

        startPos = start;
    }

    public override void Run() 
    {
        if (followPath)
        {
            elapsed += Time.fixedDeltaTime;

            if (points.Length > currentPoint)
            {
                if (elapsed > points[currentPoint].time + points[currentPoint].wait)
                {
                    elapsed -= points[currentPoint].time + points[currentPoint].wait;
                    currentPoint++;

                    startPos = projectile.transform.position;
                }

                if (points.Length > currentPoint)
                {
                    points[currentPoint].PlaceBulletAlongPath(projectile, elapsed, startPos);
                }
            }
            else
            {
                if (destroyOnFinish)
                {
                    projectile.Despawn();
                }
            }
        }
        else
        {
            if (!(tracking && pinpointLocation))
            {
                projectile.transform.Translate(Vector3.right * speed * Time.fixedDeltaTime);
                speed += acceleration * Time.fixedDeltaTime;

                if (tracking)
                {
                    Vector3 direction = PlayerMovement.current.transform.position - projectile.transform.position;

                    if (direction.normalized + projectile.transform.right == Vector3.zero)
                    {
                        direction.x += 1;
                    }

                    Vector3 newRight = Vector3.RotateTowards(projectile.transform.right, direction, rotationSpeed * Time.fixedDeltaTime, 0);

                    newRight.z = 0;

                    projectile.transform.right = newRight;
                }
            }
            else
            {
                // projectile.transform.position = PlayerMovement.current.transform.position;

                Vector3 pos1 = Vector3.zero;
                Vector3 pos2 = Vector3.zero;

                if (trackX)
                {
                    pos1 += Vector3.right * projectile.transform.position.x;
                    pos2 += Vector3.right * PlayerMovement.current.transform.position.x;
                }
                if (trackY)
                {
                    pos1 += Vector3.up * projectile.transform.position.y;
                    pos2 += Vector3.up * PlayerMovement.current.transform.position.y;
                }

                Vector3 direction = pos2 - pos1;
                float distance = direction.magnitude;

                float trackSpeed = Mathf.Pow(Mathf.Clamp01(distance/maxPSpeedDistance), 2);

                projectile.transform.position += direction * trackSpeed * pinpointSpeed * Time.fixedDeltaTime;
            }
        }
        
    }

    public override void DrawGizmos()
    {
        if (followPath)
        {
            Vector3 lastPosition = start;

            foreach (Point point in points)
            {
                lastPosition = point.DrawGizmos(lastPosition);
            }
        }
    }
}

[Serializable]
public class Point
{
    public Vector3 position; // the position of the target
    public float time; // the time in seconds that the bullet takes to traverce this path
    public float wait; // the time after reaching the destination that the bullet will wait for
    public bool orbit; // determaines if the path will be a line from the current position to the target, or if the path will circle around the target

    [ShowIf("orbit")]
    public float travel;

    public void PlaceBulletAlongPath(Projectile projectile, float t, Vector3 previous)
    {
        float traveled = t/time;

        if (!orbit)
        {
            projectile.transform.position = previous + (position-previous) * traveled;
        }
        else
        {
            Vector3 vectorAngle = previous - position;
            float angle = Mathf.Atan2(vectorAngle.y, vectorAngle.x);
            float radius = vectorAngle.magnitude;

            if (radius == 0) return;
            
            float gotoAngle = angle + travel/radius;

            float difference = gotoAngle - angle;
            float currentAngle = angle + difference * traveled;

            projectile.transform.position = new Vector3(Mathf.Cos(currentAngle)*radius, Mathf.Sin(currentAngle)*radius, 0) + position;
        }
        
    }

    public Vector3 DrawGizmos(Vector3 previous)
    {
        Gizmos.color = Color.lightGray;

        Gizmos.DrawWireSphere(position, 0.2f);
        
        if (previous != null)
        {
            if (!orbit)
            {
                Gizmos.DrawLine(position, previous);

                return position;
            }
            else
            {
                Vector3 vectorAngle = previous - position;
                float angle = Mathf.Atan2(vectorAngle.y, vectorAngle.x);
                float radius = vectorAngle.magnitude;

                if (radius == 0) return position;

                float gotoAngle = angle + travel/radius;

                Vector3 pPosition = new Vector3(Mathf.Cos(angle)*radius, Mathf.Sin(angle)*radius, 0);
                pPosition += position;

                if (travel > 0)
                {
                    for (float cAngle = angle + 0.1f; cAngle < gotoAngle; cAngle += 0.1f)
                    {
                        Vector3 cPosition = new Vector3(Mathf.Cos(cAngle)*radius, Mathf.Sin(cAngle)*radius, 0);
                        cPosition += position;

                        Gizmos.DrawLine(cPosition, pPosition);

                        pPosition = cPosition;
                    }
                }
                else
                {
                    for (float cAngle = angle - 0.1f; cAngle > gotoAngle; cAngle -= 0.1f)
                    {
                        Vector3 cPosition = new Vector3(Mathf.Cos(cAngle)*radius, Mathf.Sin(cAngle)*radius, 0);
                        cPosition += position;

                        Gizmos.DrawLine(cPosition, pPosition);

                        pPosition = cPosition;
                    }
                }
                
                Vector3 fPosition = new Vector3(Mathf.Cos(gotoAngle)*radius, Mathf.Sin(gotoAngle)*radius, 0) + position;
                Gizmos.DrawLine(pPosition, fPosition);

                return fPosition;
            }
        }

        return position;
    }
}