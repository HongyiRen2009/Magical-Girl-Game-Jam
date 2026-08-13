using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;



// coroutines dont exist in scipts not inheritting from MonoBehavior. so please dont judge me too hard

// [Serializable]
// public class M_Path : Mod
// {
//     [SerializeField] Path[] paths;
//     [SerializeField] bool destroyOnFinish = true;

//     float elapsed;

//     [Header("Gizmos")]
//     [SerializeField] bool drawGizmos;

//     public override void Begin(Projectile projectile)
//     {
//         base.Begin(projectile);
//     }

//     // IEnumerator FollowPath()
//     // {
//     //     foreach (Path path in paths)
//     //     {
//     //         if (path.orbit)
//     //         {
//     //             StartCoroutine();
//     //         }
//     //     }
//     // }

//     private void OnValidate()
//     {
//         if (!drawGizmos) return;

//         Path previous = null;
//         foreach (Path path in paths)
//         {
//             path.DrawGizmos(previous);
//             previous = path;
//         }
//     }

//     public override void Run() 
//     {
//         elapsed += Time.fixedDeltaTime;


//     }

//     public override void End() 
//     {
//         return;
//     }
    
// }

