using System;
using UnityEngine;

public class SpawnAttacks : MonoBehaviour
{
    [SerializeField] private GameObject AttackBeatmap;
    private (float, Attack[])[] AttackWaves;
	private float GameTime = 0f;
    private int currentWaveIndex = 0;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        ParseAttackWaves();
    }
    private void ParseAttackWaves()
    {
        AttackWaves = new (float, Attack[])[AttackBeatmap.transform.childCount];
        for (int i = 0; i < AttackWaves.Length; i++)
        {
            Transform AttackWave = AttackBeatmap.transform.GetChild(i);
            Attack[] attacks = new Attack[AttackWave.childCount];
			for (int j = 0; j < attacks.Length; j++)
			{
				attacks[j] = AttackWave.GetChild(j).GetComponent<Spawner>().objectToSpawn.GetComponent<Attack>();
			}
            AttackWaves[currentWaveIndex] = (AttackWave.GetComponent<AttackWave>().TimeToSpawn, attacks);
		}
        Array.Sort(AttackWaves, (a, b) => a.Item1.CompareTo(b.Item1));
    }
    // Update is called once per frame
    void Update()
    {
        GameTime += Time.deltaTime;
        if (currentWaveIndex < AttackWaves.Length && GameTime >= AttackWaves[currentWaveIndex].Item1)
        {
            foreach (var attack in AttackWaves[currentWaveIndex].Item2)
            {
                attack.ExecuteAttack();
            }
            currentWaveIndex++;
        }
    }
}
