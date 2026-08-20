using UnityEngine;
using System;
using System.ComponentModel;


[Serializable]
public class M_Spawn : Mod
{
    [SerializeField] bool runOnStart;
    [SerializeField] bool runOnEnd;
    [Tooltip("If the projectile being spawned will be set as a child of the object it is being spawned by")] [SerializeField] bool childTransformOfModded;

    [SerializeField] Attack attack;

	[SerializeField] bool drawGizmos;

    public override void Begin(Projectile projectile)
    {
        base.Begin(projectile);

        attack.parentToModded = childTransformOfModded;

        if (runOnStart) {attack.ExecuteAttack(projectile.gameObject);}
    }

    public override void End()
    {
        attack.parentToModded = childTransformOfModded;

        if (runOnEnd) 
        {
            attack.ExecuteAttack(projectile.transform.position, projectile.transform.rotation);
        }
    }

    public override void DrawGizmos(GameObject modded)
    {
        if (drawGizmos) attack.DrawGizmos(modded);
    }
}
