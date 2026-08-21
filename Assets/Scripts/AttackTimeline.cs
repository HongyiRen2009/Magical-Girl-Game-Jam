using System;
using UnityEngine;

[Serializable]
public class AttackPackage
{
    [SerializeField] public Spawn spawn;
    [SerializeField] public float commencement;
}

public class AttackTimeline : MonoBehaviour
{
    public static AttackTimeline current;

    [SerializeField] float time;
    int attackIndex;

    [SerializeField] AttackPackage[] spawns;
    [SerializeField] private AudioSource bgMusic;
    void Awake()
    {
        current = this;
    }

    void Start()
    {
    bgMusic.time = time;
        // sorts the array
        Array.Sort(spawns, (a, b) => a.commencement.CompareTo(b.commencement));
        
        // if a dev is trying to start later in the level this moves the attack index to where it should be in the level at that point
        while (time > spawns[attackIndex].commencement)
        {
            attackIndex++;
        }
    }

    void FixedUpdate()
    {
        // incriment time
        time += Time.fixedDeltaTime;

        if (attackIndex >= spawns.Length) return;

        // check if its time to spawn the next attack
        if (time > spawns[attackIndex].commencement)
        {
            spawns[attackIndex].spawn.Activate();

            attackIndex++;
        }
    }
}
