using System;
using System.Collections;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

// coroutines dont exist in scipts not inheritting from MonoBehavior. so please dont judge me too hard

[CreateAssetMenu(fileName = "PathMod", menuName = "Bullet Mods/Path Mod")]
public class M_Path : Mod
{
    [SerializeField] Path[] paths;
    [SerializeField] bool destroyOnFinish = true;

    float elapsed;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos;

    public override void Begin(Projectile projectile)
    {
        base.Begin(projectile);
    }

    // IEnumerator FollowPath()
    // {
    //     foreach (Path path in paths)
    //     {
    //         if (path.orbit)
    //         {
    //             StartCoroutine();
    //         }
    //     }
    // }

    private void OnValidate()
    {
        if (!drawGizmos) return;

        Path previous = null;
        foreach (Path path in paths)
        {
            path.DrawGizmos(previous);
            previous = path;
        }
    }

    public override void Run() 
    {
        elapsed += Time.fixedDeltaTime;


    }

    public override void End() 
    {
        return;
    }
    
}

public class Path
{
    public Vector3 position; // the position of the target
    public Vector3 time; // the time in seconds that it takes to traverce this path
    public bool orbit; // determaines if the path will be a line from the current position to the target, or if the path will circle around the target
    
    [ShowIf("orbit")]
    public bool clockwise;

    [ShowIf("orbit")]
    public float distance;

    public void PlaceBulletAlongPath(float t)
    {
        
    }

    public void DrawGizmos(Path previous = null)
    {
        Gizmos.color = Color.lightGray;

        Gizmos.DrawWireSphere(position, 0.2f);
        
        if (previous != null)
        {
            if (!orbit)
            {
                Gizmos.DrawLine(position, previous.position);
            }
            else
            {
                Vector3 vectorAngle = previous.position - position;
                float angle = Mathf.Atan2(vectorAngle.y, vectorAngle.x);

                float gotoAngle = angle + distance/vectorAngle.magnitude * (clockwise ? -1 : 1);

                Vector3 pPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

                for (float cAngle = angle + 0.1f; cAngle < gotoAngle; cAngle += 0.1f)
                {
                    Vector3 cPosition = new Vector3(Mathf.Cos(cAngle), Mathf.Sin(cAngle), 0);

                    Gizmos.DrawLine(cPosition, pPosition);

                    pPosition = cPosition;
                }

                Gizmos.DrawLine(pPosition, new Vector3(Mathf.Cos(gotoAngle), Mathf.Sin(gotoAngle), 0));
            }
        }
    }
}