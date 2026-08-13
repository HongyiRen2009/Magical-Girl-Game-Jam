using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class M_Movement : Mod
{
    [SerializeField] bool followPath;

    [AllowNesting] [HideIf("followPath")] [SerializeField] float speed;
    [AllowNesting] [HideIf("followPath")] [SerializeField] float acceleration;

    [AllowNesting] [ShowIf("followPath")] [SerializeField] Vector3 start;
    [AllowNesting] [ShowIf("followPath")] [SerializeField] Point[] points;
    [AllowNesting] [ShowIf("followPath")] [SerializeField] bool destroyOnFinish = true;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos;

    float elapsed = 0;
    int currentPoint = 0;
    // Vector3 last
    

    public override void Begin(Projectile projectile)
    {
        base.Begin(projectile);
    }

    public override void Run() 
    {
        if (followPath)
        {
            elapsed += Time.fixedDeltaTime;

            if (points.Length > currentPoint)
            {
                if (elapsed > points[currentPoint].time)
                {
                    elapsed -= points[currentPoint].time;
                    currentPoint++;
                }

                if (points.Length > currentPoint)
                {
                    if (currentPoint == 0)
                    {
                        points[currentPoint].PlaceBulletAlongPath(projectile, elapsed, start);
                    }
                    else
                    {
                        points[currentPoint].PlaceBulletAlongPath(projectile, elapsed, points[currentPoint-1].position);
                    }
                }
            }
        }
        else
        {
            projectile.transform.Translate(Vector3.right * speed * Time.fixedDeltaTime);
            speed += acceleration * Time.fixedDeltaTime;
        }
        
    }

    public override void DrawGizmos()
    {
        Vector3 previous = start;

        foreach (Point point in points)
        {
            point.DrawGizmos(previous);
            previous = point.position;
        }
    }
}

[Serializable]
public class Point
{
    public Vector3 position; // the position of the target
    public float time; // the time in seconds that it takes to traverce this path
    public bool orbit; // determaines if the path will be a line from the current position to the target, or if the path will circle around the target
    
    [ShowIf("orbit")]
    public bool clockwise;

    [ShowIf("orbit")]
    public float distance;

    public void PlaceBulletAlongPath(Projectile projectile, float t, Vector3 previous)
    {
        if (!orbit)
        {
            projectile.transform.position = previous + (position-previous) * (t/time);
        }
        
    }

    public void DrawGizmos(Vector3 previous)
    {
        Gizmos.color = Color.lightGray;

        Gizmos.DrawWireSphere(position, 0.2f);
        
        if (previous != null)
        {
            if (!orbit)
            {
                Gizmos.DrawLine(position, previous);
            }
            else
            {
                Vector3 vectorAngle = previous - position;
                float angle = Mathf.Atan2(vectorAngle.y, vectorAngle.x);
                float radius = vectorAngle.magnitude;

                float gotoAngle = angle + distance/radius * (clockwise ? -1 : 1);

                Vector3 pPosition = new Vector3(Mathf.Cos(angle)*radius, Mathf.Sin(angle)*radius, 0);
                pPosition += position;

                // for (clockwise ? (float cAngle = angle + 0.1f; cAngle < gotoAngle; cAngle += 0.1f) : (float cAngle = angle + 0.1f; cAngle < gotoAngle; cAngle += 0.1f))
                // {
                //     Vector3 cPosition = new Vector3(Mathf.Cos(cAngle)*radius, Mathf.Sin(cAngle)*radius, 0);
                //     cPosition += position;

                //     Gizmos.DrawLine(cPosition, pPosition);

                //     pPosition = cPosition;
                // }

                Gizmos.DrawLine(pPosition, new Vector3(Mathf.Cos(gotoAngle)*radius, Mathf.Sin(gotoAngle)*radius, 0) + position);
            }
        }
    }
}