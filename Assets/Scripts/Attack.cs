using System.Collections;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
public enum SpawnShape{
	Point,
	Line,
}
public class Attack : MonoBehaviour
{
	// Prefab
	[SerializeField] GameObject projectilePrefab; // the prefab of the bullet that will be spawned

	// projectile attributes
	[SerializeField] float speed = 1; // speed
	[SerializeField] float acceleration = 1; // acceleration
	[SerializeField] float lifetime = 1; // lifetime

	[SerializeField] bool useSpray = true; // determines if the attack will use the spray
	[SerializeField] bool useBursts = false; // determines if the attack will use the burst
	// spawning variables
	[Header("Spray")]
	[ShowIf("useSpray")]
	[SerializeField] float duration = 1; // the time in seconds in which bullets will be spawned
	[ShowIf("useSpray")]
	[SerializeField] float delay = 0.1f; // the time in seconds between individual bullets being spawned
	[ShowIf("useSpray")]
	[SerializeField] private bool useSpread = true;
	[ShowIf(EConditionOperator.And, "useSpray", "useSpread")]
	[SerializeField] float spread; // the degree offset (positive or negitive) by which the bullet can deviate from its starting angle
	[ShowIf("useSpray")]
	[SerializeField] float distanceVariability; // the position offset (positive or negitive) relitive to the angle which the bullet can spawn at

	[Header("Bursts")]
	[ShowIf("useBursts")]
	[SerializeField] Burst[] bursts; // bullet bursts that spawn multiple bullets in an instant
	[ShowIf("useBursts")]
	[SerializeField] int burstDrawIndex = 0; // the index of the burst that will draw a gizmo in the editor. Anything outside the range of the array will not draw a gizmo.

	[Header("Shape")]
	[SerializeField] SpawnShape spawnShape = SpawnShape.Point; // the shape of the spawn area
	[ShowIf("spawnShape", SpawnShape.Line)]
	[SerializeField] float spawnLineLength = 1; // the length of the line that the bullets will spawn along
	[System.Serializable]
	public struct Burst
	{
		 public int projectileNum; // the number of projectiles in the burst
		 public float delay; // the delay in seconds that the burst occurs after the attack starts
		 public bool spawnsEvenly; // determines if the bullets spawned will spawn evenly across any spacing random-ness present in the attack
		public float spread; // the total spread of the burst
	}

	float age; // the time that the attack has been occuring for
    public void ExecuteAttack()
	{
		StartCoroutine(ExecuteSpray());
		StartCoroutine(ExecuteBursts());
	}
	private IEnumerator ExecuteSpray(){
		if(!useSpray)
		{
			yield break;
		}
		for (int i = 0;i<duration/ delay; i++)
		{
			float randomAngleOffset = useSpread ? Random.Range(-spread / 2, spread / 2) : 0;
			float randomDistanceOffset = Random.Range(-distanceVariability / 2, distanceVariability / 2);
			Vector3 spawnPosition = transform.position;
			switch(spawnShape){
				case SpawnShape.Point:
					break;
				case SpawnShape.Line:
					spawnPosition += transform.up * Random.Range(-spawnLineLength / 2, spawnLineLength / 2);
					break;
			}
			spawnPosition += transform.right * randomDistanceOffset;
			GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + randomAngleOffset)));
			projectile.GetComponent<Projectile>().Initialize(speed, acceleration, lifetime);
			yield return new WaitForSeconds(delay);
		}

	}
	private IEnumerator ExecuteBursts(){
		if(!useBursts || bursts.Length == 0)
		{
			yield break;
		}
		for (int i = 0; i < bursts.Length; i++)
		{
			float currentAngleOffset = -bursts[i].spread / 2;
			for (int j = 0; j < bursts[i].projectileNum; j++)
			{
				float randomAngleOffset = Random.Range(-spread / 2, spread / 2);
				GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(new Vector3(0, 0,transform.eulerAngles.z+ (bursts[i].spawnsEvenly ? currentAngleOffset : randomAngleOffset))));
				projectile.GetComponent<Projectile>().Initialize(speed, acceleration, lifetime);
				currentAngleOffset += bursts[i].spread / (bursts[i].projectileNum - 1);
			}
			yield return new WaitForSeconds(bursts[i].delay);
		}
	}

    void OnDrawGizmos()
    {

		Gizmos.color = Color.coral;
		Handles.color = Color.coral;
		switch(spawnShape)
		{
			case SpawnShape.Point:
				Gizmos.DrawWireSphere(transform.position, 0.2f);
				break;
			case SpawnShape.Line:
				Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
				Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
				Gizmos.DrawLine(lineStart, lineEnd);
				break;
		}
		if (useSpray)
		{
			
			// Draw a cone for the spread of the attack
			Vector3 FirstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
			Vector3 SecondEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
			Vector3 Center = transform.position + new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0) * speed * lifetime;
			switch(spawnShape)
			{
				case SpawnShape.Point:
					if (useSpread)
					{
						Gizmos.DrawLine(transform.position, FirstEnd);
						Gizmos.DrawLine(transform.position, SecondEnd);
						#if UNITY_EDITOR
							Handles.DrawWireArc(transform.position, Vector3.forward, FirstEnd - transform.position, -spread, speed * lifetime);
						#endif
					}
					else
					{
						Gizmos.DrawLine(transform.position, Center);

					}
					break;
				case SpawnShape.Line:
					Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
					Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
					Gizmos.DrawLine(lineStart, lineEnd);
					if (useSpread)
					{
						Gizmos.DrawLine(lineEnd, FirstEnd);
						Gizmos.DrawLine(lineStart, SecondEnd);
						#if UNITY_EDITOR
							Handles.DrawWireArc(transform.position, Vector3.forward, FirstEnd - transform.position, -spread, speed * lifetime);
						#endif
					}
					else
					{
						Vector3 lineOffset = transform.right * speed * lifetime;
						Gizmos.DrawLine(lineStart, lineStart+lineOffset);
						Gizmos.DrawLine(lineEnd, lineEnd+ lineOffset);
						Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);
					}
					break;
			}



		}
		// Draw a cone for the spread of the burst or draw lines for each projectile in the burst if spreads evenly
		if (burstDrawIndex < 0 || burstDrawIndex >= bursts.Length || !useBursts)
		{
			return;
		}
		Burst burst = bursts[burstDrawIndex];
		if (burst.spawnsEvenly)
		{
			float currentAngleOffset = -burst.spread / 2;
			for (int j = 0; j < burst.projectileNum; j++)
			{
				Vector3 burstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffset : 0)) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffset : 0)) * Mathf.Deg2Rad), 0) * speed * lifetime;
				Gizmos.DrawLine(transform.position, burstEnd);
				currentAngleOffset += burst.spread / (burst.projectileNum - 1);
			}
		}
		else{
			float burstspread = burst.spread;
			Vector3 BurstFirstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
			Vector3 BurstSecondEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
			Vector3 BurstCenter = transform.position + new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0) * speed * lifetime;
			switch(spawnShape)
			{
				case SpawnShape.Point:
					Gizmos.DrawLine(transform.position, BurstFirstEnd);
					Gizmos.DrawLine(transform.position, BurstSecondEnd);
					break;
				case SpawnShape.Line:
					Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
					Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
					Gizmos.DrawLine(lineEnd, BurstFirstEnd);
					Gizmos.DrawLine(lineStart, BurstSecondEnd);
					Gizmos.DrawLine(lineStart, lineEnd);
					break;
			}
#if UNITY_EDITOR
			Handles.DrawWireArc(transform.position, Vector3.forward, BurstFirstEnd - transform.position, -burstspread, speed * lifetime);
			#endif
		}

	}
}
