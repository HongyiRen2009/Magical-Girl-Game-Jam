using System;
using UnityEngine;

[Serializable]
public class AttackPackage
{
    [SerializeField] public Attack attack;
    [SerializeField] public float commencement;
}

public class AttackTimeline : MonoBehaviour
{
    [SerializeField] float time;
    int attackIndex;

    [SerializeField] AttackPackage[] attacks;

    void Start()
    {
        // sorts the array
        Array.Sort(attacks, (a, b) => a.commencement.CompareTo(b.commencement));
        
        // if a dev is trying to start later in the level this moves the attack index to where it should be in the level at that point
        while (time > attacks[attackIndex].commencement)
        {
            attackIndex++;
        }
    }

    void FixedUpdate()
    {
        // incriment time
        time += Time.fixedDeltaTime;

        // check if its time to spawn the next attack
        if (time > attacks[attackIndex].commencement)
        {
            attacks[attackIndex].attack.ExecuteAttack();

            attackIndex++;
        }
    }
}
