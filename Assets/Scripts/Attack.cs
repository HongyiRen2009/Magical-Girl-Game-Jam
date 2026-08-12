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
	[SerializeField,Tooltip("The speed of the projectile")] float speed = 1; // speed
	[SerializeField,Tooltip("The acceleration of the projectile")] float acceleration = 1; // acceleration
	[SerializeField,Tooltip("The lifetime of the projectile")] float lifetime = 1; // lifetime

	[SerializeField, Tooltip("Whether the attack will use sprays. Sprays can only fire projectiles one at a time")] bool useSpray = true; // determines if the attack will use the spray
	[SerializeField, Tooltip("Whether the attack will use bursts. Bursts can fire multiple projectiles at once")] bool useBursts = false; // determines if the attack will use the burst
	// spawning variables
	[Header("Spray")]
	[ShowIf("useSpray"), Tooltip("Duration of the spray attack")]
	[SerializeField] float duration = 1; // the time in seconds in which bullets will be spawned
	[ShowIf("useSpray"), Tooltip("The time in seconds between individual bullets being spawned")]
	[SerializeField] float delay = 0.1f; // the time in seconds between individual bullets being spawned
	[ShowIf("useSpray"), Tooltip("Whether the spray will use spread. Spread allows bullets to deviate from their starting angle")]
	[SerializeField] private bool useSpread = true;
	[ShowIf(EConditionOperator.And, "useSpray", "useSpread"), Tooltip("The degree offset by which the bullet can deviate from its starting angle")]
	[SerializeField] float spread; // the degree offset (positive or negitive) by which the bullet can deviate from its starting angle
	[ShowIf("spawnShape", SpawnShape.Line), Tooltip("Bullets further away from the line's center will angle away from the lines center if true.")]
	[SerializeField] bool useDeterministicSpread = true;
	[ShowIf("useSpray"), Tooltip("The position offset (forwards or backwards) at which the bullet can spawn")]
	[SerializeField] float distanceVariability; // the position offset (positive or negitive) relitive to the angle which the bullet can spawn at

	[Header("Bursts")]
	[ShowIf("useBursts"), Tooltip("Bursts are individual groups of projectiles that spawn multiple bullets in an instant")]
	[SerializeField] Burst[] bursts; // bullet bursts that spawn multiple bullets in an instant
	[ShowIf("useBursts"), Tooltip("Which burst to show for the gizmo")]
	[SerializeField] int burstDrawIndex = 0; // the index of the burst that will draw a gizmo in the editor. Anything outside the range of the array will not draw a gizmo.

	[Header("Shape"), Tooltip("The shape of the spawn area")]
	[SerializeField] SpawnShape spawnShape = SpawnShape.Point; // the shape of the spawn area
	[ShowIf("spawnShape", SpawnShape.Line), Tooltip("How long the line that the bullets will spawn along is")]
	[SerializeField] float spawnLineLength = 1; // the length of the line that the bullets will spawn along
	[System.Serializable]
	public class Burst
	{
		[Tooltip("The number of projectiles fired per burst fire")]
		public int projectileNum = 1; // the number of projectiles in the burst
		[Tooltip("The delay in seconds until the next burst")]
		public float endDelay = 0.2f; // the delay in seconds that the next burst occurs after the previous burst ends
		[Tooltip("Whether the spray will use spread. Spread allows bullets to deviate from their starting angle")]
		public bool spawnsEvenly = false; // determines if the bullets spawned will spawn evenly across any spacing random-ness present in the attack
		[Tooltip("The degree offset by which the bullet can deviate from its starting angle")]
		public float spread = 0; // the total spread of the burst
		[Tooltip("The total number of times the burst will repeat before going on to the next one in the list")]
		public int repeatNums = 1; // the number of times the burst will repeat
		[Tooltip("The delay in seconds for the next burst repetition to occur after the previous burst repetition ends")]
		public float delay = 0.2f; // the delay in seconds for the next burst repetition to occur after the previous burst repetition ends
		[ShowIf("spawnsEvenly", false),Tooltip("Whether to use spread. Spread allows bullets to deviate from their starting angle")]
		public bool useSpread = true;
		[ShowIf("useSpread"), Tooltip("Bullets further away from the line's center will angle away from the lines center if true.")]
		public bool useDeterministicSpread = true;
	}

	float age; // the time that the attack has been occuring for
//     public void ExecuteAttack()
// 	{
// 		StartCoroutine(ExecuteSpray());
// 		StartCoroutine(ExecuteBursts());
// 	}
// 	private IEnumerator ExecuteSpray(){
// 		if(!useSpray)
// 		{
// 			yield break;
// 		}
// 		for (int i = 0;i<duration/ delay; i++)
// 		{
// 			float randomAngleOffset = useSpread ? Random.Range(-spread / 2, spread / 2) : 0;
// 			float randomDistanceOffset = Random.Range(-distanceVariability / 2, distanceVariability / 2);
// 			Vector3 spawnPosition = transform.position;
// 			switch(spawnShape){
// 				case SpawnShape.Point:
// 					break;
// 				case SpawnShape.Line:
// 					float lineOffset = Random.Range(-spawnLineLength / 2, spawnLineLength / 2);
// 					spawnPosition += transform.up * lineOffset;
// 					if(useDeterministicSpread)
// 					{
// 						randomAngleOffset = lineOffset / spawnLineLength * spread;
// 					}
// 					break;
// 			}
// 			spawnPosition += transform.right * randomDistanceOffset;
// 			GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + randomAngleOffset)));
// 			projectile.GetComponent<Projectile>().Initialize(speed, acceleration, lifetime);
// 			yield return new WaitForSeconds(delay);
// 		}

// 	}
// 	private IEnumerator ExecuteBursts(){
// 		if(!useBursts || bursts.Length == 0)
// 		{
// 			yield break;
// 		}
// 		for (int i = 0; i < bursts.Length; i++)
// 		{
// 			for (int k = 0; k < bursts[i].repeatNums; k++)
// 			{
// 				float currentAngleOffset = -bursts[i].spread / 2;
// 				for (int j = 0; j < bursts[i].projectileNum; j++)
// 				{
// 					float randomAngleOffset = bursts[i].useSpread ? Random.Range(-bursts[i].spread / 2, bursts[i].spread / 2) : 0;
// 					float randomDistanceOffset = Random.Range(-distanceVariability / 2, distanceVariability / 2);
// 					Vector3 spawnPosition = transform.position;
// 					switch (spawnShape)
// 					{
// 						case SpawnShape.Point:
// 							break;
// 						case SpawnShape.Line:
// 							float lineOffset = bursts[i].spawnsEvenly ? -spawnLineLength / 2 + j * (spawnLineLength / (bursts[i].projectileNum - 1)) : Random.Range(-spawnLineLength / 2, spawnLineLength / 2);
// 							spawnPosition += transform.up * lineOffset;
// 							if (bursts[i].useDeterministicSpread)
// 							{
// 								randomAngleOffset = lineOffset / spawnLineLength * bursts[i].spread;
// 							}
// 							break;
// 					}
// 					GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + (bursts[i].spawnsEvenly ? currentAngleOffset : randomAngleOffset))));
// 					projectile.GetComponent<Projectile>().Initialize(speed, acceleration, lifetime);
// 					currentAngleOffset += bursts[i].spread / (bursts[i].projectileNum - 1);
// 				}
// 				yield return new WaitForSeconds(bursts[i].delay);
// 			}
// 			yield return new WaitForSeconds(bursts[i].endDelay);
// 		}
// 	}

//     void OnDrawGizmos()
//     {

// 		Gizmos.color = Color.red;
// 		Handles.color = Color.red;
// 		switch(spawnShape)
// 		{
// 			case SpawnShape.Point:
// 				Gizmos.DrawWireSphere(transform.position, 0.2f);
// 				break;
// 			case SpawnShape.Line:
// 				Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
// 				Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
// 				Gizmos.DrawLine(lineStart, lineEnd);
// 				break;
// 		}
// 		if (useSpray)
// 		{
			
// 			// Draw a cone for the spread of the attack
// 			Vector3 FirstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			Vector3 SecondEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			Vector3 Center = transform.position + new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			switch(spawnShape)
// 			{
// 				case SpawnShape.Point:
// 					if (useSpread)
// 					{
// 						Gizmos.DrawLine(transform.position, FirstEnd);
// 						Gizmos.DrawLine(transform.position, SecondEnd);
// 						#if UNITY_EDITOR
// 							Handles.DrawWireArc(transform.position, Vector3.forward, FirstEnd - transform.position, -spread, speed * lifetime);
// 						#endif
// 					}
// 					else
// 					{
// 						Gizmos.DrawLine(transform.position, Center);

// 					}
// 					break;
// 				case SpawnShape.Line:
// 					Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
// 					Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
// 					Vector3 lineFirstEnd = lineEnd + new Vector3(Mathf.Cos((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + spread / 2) * Mathf.Deg2Rad),0) * speed * lifetime;
// 					Vector3 lineSecondEnd = lineStart + new Vector3(Mathf.Cos((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - spread / 2) * Mathf.Deg2Rad),0) * speed * lifetime;
// 					Gizmos.DrawLine(lineStart, lineEnd);
// 					if (useSpread)
// 					{
// 						Gizmos.DrawLine(lineEnd, lineFirstEnd);
// 						Gizmos.DrawLine(lineStart, lineSecondEnd);
// #if UNITY_EDITOR
// 						Vector3 center = (lineEnd + lineStart) / 2;
// 						float targetDistance = speed * lifetime;
// 						float baseAngleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
// 						Vector3 bisectorDir = new Vector3(Mathf.Cos(baseAngleRad), Mathf.Sin(baseAngleRad), 0);
// 						Vector3 bezierCenterTarget = center + bisectorDir * targetDistance;
// 						Vector3 chordMidpoint = (lineFirstEnd + lineSecondEnd) / 2f;
// 						Vector3 controlPoint = (4f * bezierCenterTarget - chordMidpoint) / 3f;
// 						Handles.DrawBezier(
// 							lineSecondEnd,
// 							lineFirstEnd,
// 							controlPoint,
// 							controlPoint,
// 							Handles.color,
// 							null,
// 							2f
// 						);
// #endif

// 					}
// 					else
// 					{
// 						Vector3 lineOffset = transform.right * speed * lifetime;
// 						Gizmos.DrawLine(lineStart, lineStart+lineOffset);
// 						Gizmos.DrawLine(lineEnd, lineEnd+ lineOffset);
// 						Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);
// 					}
// 					break;
// 			}



// 		}
// 		// Draw a cone for the spread of the burst or draw lines for each projectile in the burst if spreads evenly
// 		if (burstDrawIndex < 0 || burstDrawIndex >= bursts.Length || !useBursts)
// 		{
// 			return;
// 		}
// 		Burst burst = bursts[burstDrawIndex];
// 		if (burst.spawnsEvenly)
// 		{
// 			switch (spawnShape) {
// 				case SpawnShape.Point:
// 					float currentAngleOffset = -burst.spread / 2;
// 					for (int j = 0; j < burst.projectileNum; j++)
// 					{
// 						Vector3 burstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffset : 0)) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffset : 0)) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 						Gizmos.DrawLine(transform.position, burstEnd);
// 						currentAngleOffset += burst.spread / (burst.projectileNum - 1);
// 					}
// 					break;
// 				case SpawnShape.Line:
// 					float currentAngleOffsetLine = -burst.spread / 2;
// 					float currentSpawnOffset = -spawnLineLength / 2;
// 					for (int j = 0; j < burst.projectileNum; j++)
// 					{
// 						Vector3 spawnPosition = transform.position + transform.up * currentSpawnOffset;
// 						Vector3 burstEnd = spawnPosition + new Vector3(Mathf.Cos((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffsetLine : 0)) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + (burst.spawnsEvenly ? currentAngleOffsetLine : 0)) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 						Gizmos.DrawLine(spawnPosition, burstEnd);
// 						currentAngleOffsetLine += burst.spread / (burst.projectileNum - 1);
// 						currentSpawnOffset += spawnLineLength / (burst.projectileNum - 1);
// 					} 
// 					break;
// 			}

// 		}
// 		else{
// 			float burstspread = burst.spread;
// 			Vector3 BurstFirstEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			Vector3 BurstSecondEnd = transform.position + new Vector3(Mathf.Cos((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			Vector3 BurstCenter = transform.position + new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0) * speed * lifetime;
// 			switch (spawnShape)
// 			{
// 				case SpawnShape.Point:
// 					if (burst.useSpread)
// 					{
// 						Gizmos.DrawLine(transform.position, BurstFirstEnd);
// 						Gizmos.DrawLine(transform.position, BurstSecondEnd);
// #if UNITY_EDITOR
// 						Handles.DrawWireArc(transform.position, Vector3.forward, BurstFirstEnd - transform.position, -burstspread, speed * lifetime);
// #endif
// 					}
// 					else
// 					{
// 						Gizmos.DrawLine(transform.position, BurstCenter);

// 					}
// 					break;
// 				case SpawnShape.Line:
// 					Vector3 lineStart = transform.position - transform.up * spawnLineLength / 2;
// 					Vector3 lineEnd = transform.position + transform.up * spawnLineLength / 2;
// 					Vector3 lineFirstEnd = lineEnd + new Vector3(Mathf.Cos((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 					Vector3 lineSecondEnd = lineStart + new Vector3(Mathf.Cos((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z - burstspread / 2) * Mathf.Deg2Rad), 0) * speed * lifetime;
// 					Gizmos.DrawLine(lineStart, lineEnd);
// 					if (burst.useSpread)
// 					{
// 						Gizmos.DrawLine(lineEnd, lineFirstEnd);
// 						Gizmos.DrawLine(lineStart, lineSecondEnd);
// #if UNITY_EDITOR
// 						Vector3 center = (lineEnd + lineStart) / 2;
// 						float targetDistance = speed * lifetime;
// 						float baseAngleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
// 						Vector3 bisectorDir = new Vector3(Mathf.Cos(baseAngleRad), Mathf.Sin(baseAngleRad), 0);
// 						Vector3 bezierCenterTarget = center + bisectorDir * targetDistance;
// 						Vector3 chordMidpoint = (lineFirstEnd + lineSecondEnd) / 2f;
// 						Vector3 controlPoint = (4f * bezierCenterTarget - chordMidpoint) / 3f;
// 						Handles.DrawBezier(
// 							lineSecondEnd,
// 							lineFirstEnd,
// 							controlPoint,
// 							controlPoint,
// 							Handles.color,
// 							null,
// 							2f
// 						);
// #endif

// 					}
// 					else
// 					{
// 						Vector3 lineOffset = transform.right * speed * lifetime;
// 						Gizmos.DrawLine(lineStart, lineStart + lineOffset);
// 						Gizmos.DrawLine(lineEnd, lineEnd + lineOffset);
// 						Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);
// 					}
// 					break;
// 			}
// 		}

// 	}
}
