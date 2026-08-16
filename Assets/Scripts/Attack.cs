using NaughtyAttributes;
using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using Unity.Mathematics;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using NUnit.Framework.Internal.Builders;

public enum SpawnShape{
	Point,
	Line,
}

[Serializable]
public class Burst
{
	[Tooltip("The number of projectiles in the burst")] public int projectileNum;
	[Tooltip("The time in seconds in which the burst occurs")] public float delay;

	[Header("Repetition")]
	[Tooltip("The number of times the burst will repeat")] public int repeat = 0;
	[Tooltip("The delay in seconds for the next burst repetition to occur after the previous burst repetition ends")] public float repeatDelay = 1;

	[Header("Spread")]
	[Tooltip("The degree offset by which the bullet can deviate from its starting angle")] public float spread;
	[Tooltip("Bullets will spawn an equal disance away from each other if true")] public bool useEvenSpawn;
	[Tooltip("Bullets further away from the line's center will angle away from the lines center if true.")] public bool useDeterministicSpread;

	[Header("Gizmos")]
	public bool showGizmos;
}
public class Attack : MonoBehaviour
{
	// Prefab
	[SerializeField] GameObject projectilePrefab; // the prefab of the bullet that will be spawned

	[Header("Bullet Settings")]
	[SerializeField] [Tooltip("The lifetime of the projectile. Set to 0 for infinite lifetime")] float lifetime; // lifetime
	[SerializeField] [Tooltip("Whether or not the projectile is active")] bool active = true; // lifetime
	[SerializeReference] [SubclassSelector] private Mod[] mods; // the mods that will be applied to the projectile when it is spawned

	[Header("Spray")]
	[Tooltip("The time in seconds that projectiles are spawned for")] [SerializeField] float duration;
	[Tooltip("The time in seconds between individual projectiles being spawned")] [SerializeField] float firerate = 1f;
	[Tooltip("The time in seconds that elapses before projectiles spawn")] [SerializeField] float delay;
	[Tooltip("The degree offset by which the projectile can deviate from its starting angle")] [SerializeField] float spread;
	[ShowIf("spawnShape", SpawnShape.Line), Tooltip("Projectiles further away from the line's center will angle away from the lines center if true.")] [SerializeField] bool useDeterministicSpread = true;

	[Header("Bursts")]
	[Tooltip("Spawn multiple bullets in an instant")] [SerializeField] Burst[] bursts;

	[Header("Shape")]
	[Tooltip("The shape of the spawn area")] [SerializeField] SpawnShape spawnShape;
	[ShowIf("spawnShape", SpawnShape.Line), Tooltip("How long the line that the bullets will spawn along is")] [SerializeField] float spawnLineLength = 1; // the length of the line that the bullets will spawn along

	[Header("Gizmos")]
	[SerializeField] bool showGizmos;
	[ShowIf("showGizmos")] [SerializeField] bool showSpray;
	float gizmosLength = 2f;

    public void ExecuteAttack()
	{
		StartCoroutine(ExecuteSpray());

		foreach (Burst burst in bursts)
		{
			StartCoroutine(ExecuteBurst(burst));
		}
	}

	// preforms a complete spray of bullets
	private IEnumerator ExecuteSpray()
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(delay);


		float elapsed = 0; // time sense spray has started
		float lastSpawn = 0; // time sense last projectile was spawned

		while (elapsed < duration)
		{
			// incriments the time
			elapsed += Time.deltaTime;
			lastSpawn += Time.deltaTime;

			if (lastSpawn > firerate)
			{
				SpawnProjectile();

				lastSpawn -= firerate;
			}

			// waits for next frame
			yield return null;
		}
	}

	// spawns a projectile (baised on the initilized spray values)
	void SpawnProjectile()
	{
		float randomAngleOffset = UnityEngine.Random.Range(-spread / 2, spread / 2);

		Vector3 spawnPosition = transform.position;

		if (spawnShape == SpawnShape.Line)
		{
			float lineOffset = UnityEngine.Random.Range(-spawnLineLength / 2, spawnLineLength / 2);
			spawnPosition += transform.up * lineOffset; // could be a bug

			if (useDeterministicSpread)
			{
				randomAngleOffset = lineOffset / spawnLineLength * spread;
			}
		}

		Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + randomAngleOffset));

		GameObject projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

		Mod[] deepCopiedMods = new Mod[mods.Length];
		for (int i = 0; i < mods.Length; i++)
		{
			deepCopiedMods[i] = (Mod) mods[i].Clone();
		}
		projectile.GetComponent<Projectile>().Initialize(lifetime, deepCopiedMods, active);
	}

	private IEnumerator ExecuteBurst(Burst burst)
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(burst.delay);

		int iteration = 0;

		while (burst.repeat >= iteration)
		{
			iteration++;

			SpawnBurst(burst);
			
			yield return new WaitForSeconds(burst.repeatDelay);
		}
	}

	void SpawnBurst(Burst burst)
	{
		for (int proj = 0; proj < burst.projectileNum; proj++)
		{
			float randomAngleOffset = UnityEngine.Random.Range(-burst.spread / 2, burst.spread / 2);

			if (burst.useDeterministicSpread)
			{
				float progression = proj/(burst.projectileNum-1f);
				progression -= 0.5f;

				randomAngleOffset = burst.spread * progression;
			}

			Vector3 spawnPosition = transform.position;

			if (spawnShape == SpawnShape.Line)
			{
				float lineOffset = UnityEngine.Random.Range(-spawnLineLength / 2, spawnLineLength / 2);

				if (burst.useEvenSpawn)
				{
					float progression = proj/((float) burst.projectileNum-1);
					progression -= 0.5f;

					lineOffset = spawnLineLength * progression;
				}

				spawnPosition += transform.up * lineOffset;
			}

			Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + randomAngleOffset));

			GameObject projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

			Mod[] deepCopiedMods = new Mod[mods.Length];
			for (int i = 0; i < mods.Length; i++)
			{
				deepCopiedMods[i] = (Mod) mods[i].Clone();
			}
			projectile.GetComponent<Projectile>().Initialize(lifetime, deepCopiedMods, active);
		}
	}

    void OnDrawGizmos()
    {
		if (!showGizmos)
		{
			return;
		}

		Gizmos.color = Color.red;
		Handles.color = Color.red;

		// drawing the attack shape
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

		foreach (Mod mod in mods)
		{
			mod.DrawGizmos();
		}

		// drawing spray
		if (showSpray)
		{
			DrawSpray();
		}
		// Draw a cone for the spread of the burst or draw lines for each projectile in the burst if spreads evenly

		foreach (Burst burst in bursts)
		{
			if (burst.showGizmos)
			{
				DrawBurst(burst);
			}
		}
	}

	void DrawSpray()
	{
		Gizmos.color = Color.red;
		Vector3 vRot = transform.eulerAngles * Mathf.Deg2Rad; // stands for vector rotation
		
		// if there is NO spread
		if (spread == 0)
		{
			switch (spawnShape)
			{
				// point, no spread
				case SpawnShape.Point:

					Vector3 Center = transform.position + new Vector3(Mathf.Cos(vRot.z), Mathf.Sin(vRot.z), 0) * gizmosLength;
					Gizmos.DrawLine(transform.position, Center);

					return;
				
				// line, no spread
				case SpawnShape.Line:

					float hLen = spawnLineLength / 2; // stands for half line length

					Vector3 lineStart = transform.position - transform.up * hLen;
					Vector3 lineEnd = transform.position + transform.up * hLen;

					Vector3 lineOffset = transform.right * gizmosLength;

					Gizmos.DrawLine(lineStart, lineStart + lineOffset);
					Gizmos.DrawLine(lineEnd, lineEnd + lineOffset);
					Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);

					return;
			}
		}

		// if there is spread
		else
		{
			float hSpr = spread/2 * Mathf.Deg2Rad; // stands for half spread

			switch(spawnShape)
			{
				// point, yes spread
				case SpawnShape.Point:

					Vector3 FirstEnd = new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
					Vector3 SecondEnd = new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;

					Gizmos.DrawLine(transform.position, transform.position + FirstEnd);
					Gizmos.DrawLine(transform.position, transform.position + SecondEnd);

					#if UNITY_EDITOR
						Handles.DrawWireArc(transform.position, Vector3.forward, FirstEnd, -spread, gizmosLength);
					#endif
					
					return;

				// line, yes spread
				case SpawnShape.Line:

					float hLen = spawnLineLength / 2; // stands for half line length

					Vector3 lineStart = transform.position - transform.up * hLen;
					Vector3 lineEnd = transform.position + transform.up * hLen;

					Vector3 lineStartExtention = lineStart + new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;
					Vector3 lineEndExtention = lineEnd + new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
					

					// Gizmos.DrawLine(lineStart, lineEnd);
					Gizmos.DrawLine(lineStart, lineStartExtention);
					Gizmos.DrawLine(lineEnd, lineEndExtention);
					
					#if UNITY_EDITOR
					Vector3 center = (lineEnd + lineStart) / 2;
					float baseAngleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
					Vector3 bisectorDir = new Vector3(Mathf.Cos(baseAngleRad), Mathf.Sin(baseAngleRad), 0);
					Vector3 bezierCenterTarget = center + bisectorDir * gizmosLength;
					Vector3 chordMidpoint = (lineStartExtention + lineEndExtention) / 2f;
					Vector3 controlPoint = (4f * bezierCenterTarget - chordMidpoint) / 3f;
					Handles.DrawBezier(
						lineStartExtention,
						lineEndExtention,
						controlPoint,
						controlPoint,
						Handles.color,
						null,
						2f
					);
					#endif

					return;
			}
		}
	}

	void DrawBurst(Burst burst)
	{
		Gizmos.color = Color.red;
		Vector3 vRot = transform.eulerAngles * Mathf.Deg2Rad; // stands for vector rotation
		
		// if there is NO spread
		if (burst.spread == 0)
		{
			Vector3 Center = new Vector3(Mathf.Cos(vRot.z), Mathf.Sin(vRot.z), 0) * gizmosLength;

			switch (spawnShape)
			{
				// point, no spread
				case SpawnShape.Point:

					Gizmos.DrawLine(transform.position, transform.position + Center);

					for (int i = 0; i < burst.projectileNum; i++)
					{
						Gizmos.DrawSphere(transform.position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
					}

					return;
				
				// line, no spread
				case SpawnShape.Line:
					
					Vector3 lineOffset = transform.right * gizmosLength;

					if (burst.useEvenSpawn)
					{
						for (int i = 0; i < burst.projectileNum; i++)
						{
							float progression = i/((float) burst.projectileNum-1) - 0.5f;
							Vector3 linePosition = transform.position + transform.up * spawnLineLength * progression;

							Gizmos.DrawLine(linePosition, linePosition + lineOffset);
						}
					}
					else
					{
						float hLen = spawnLineLength / 2; // stands for half line length

						Vector3 lineStart = transform.position - transform.up * hLen;
						Vector3 lineEnd = transform.position + transform.up * hLen;

						Gizmos.DrawLine(lineStart, lineStart + lineOffset);
						Gizmos.DrawLine(lineEnd, lineEnd + lineOffset);
						Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);
						
						for (int i = 0; i < burst.projectileNum; i++)
						{
							Gizmos.DrawSphere(transform.position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
						}
					}
					return;

			}
		}

		// if there is spread
		else
		{
			float hSpr = burst.spread/2 * Mathf.Deg2Rad; // stands for half spread

			Vector3 Center = new Vector3(Mathf.Cos(vRot.z), Mathf.Sin(vRot.z), 0) * gizmosLength;

			switch(spawnShape)
			{
				// point, yes spread
				case SpawnShape.Point:

					if (burst.useDeterministicSpread)
					{
						for (int i = 0; i < burst.projectileNum; i++)
						{
							float progression = i/((float) burst.projectileNum-1) - 0.5f;
							float angle = progression * burst.spread * Mathf.Deg2Rad;

							Vector3 end = new Vector3(Mathf.Cos(vRot.z + angle), Mathf.Sin(vRot.z + angle), 0) * gizmosLength;

							Gizmos.DrawLine(transform.position, transform.position + end);
						}
					}
					else
					{
						Vector3 FirstEnd = new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
						Vector3 SecondEnd = new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;

						Gizmos.DrawLine(transform.position, transform.position + FirstEnd);
						Gizmos.DrawLine(transform.position, transform.position + SecondEnd);

						#if UNITY_EDITOR
							Handles.DrawWireArc(transform.position, Vector3.forward, FirstEnd, -burst.spread, gizmosLength);
						#endif

						for (int i = 0; i < burst.projectileNum; i++)
						{
							Gizmos.DrawSphere(transform.position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
						}
					}
					
					return;

				// line, yes spread
				case SpawnShape.Line:

					if (burst.useDeterministicSpread && burst.useEvenSpawn)
					{
						for (int i = 0; i < burst.projectileNum; i++)
						{
							float progression = i/((float) burst.projectileNum-1) - 0.5f;

							Vector3 linePosition = transform.position + transform.up * spawnLineLength * progression;

							float angle = progression * burst.spread * Mathf.Deg2Rad;
							Vector3 lineEnd = new Vector3(Mathf.Cos(vRot.z + angle), Mathf.Sin(vRot.z + angle), 0) * gizmosLength;

							Gizmos.DrawLine(linePosition, linePosition + lineEnd);
						}
					}
					else
					{
						float hLen = spawnLineLength / 2; // stands for half line length

						Vector3 lineStart = transform.position - transform.up * hLen;
						Vector3 lineEnd = transform.position + transform.up * hLen;

						Vector3 lineStartExtention = lineStart + new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;
						Vector3 lineEndExtention = lineEnd + new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
						
						Gizmos.DrawLine(lineStart, lineStartExtention);
						Gizmos.DrawLine(lineEnd, lineEndExtention);
						
						#if UNITY_EDITOR
							Vector3 center = (lineEnd + lineStart) / 2;
							float baseAngleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
							Vector3 bisectorDir = new Vector3(Mathf.Cos(baseAngleRad), Mathf.Sin(baseAngleRad), 0);
							Vector3 bezierCenterTarget = center + bisectorDir * gizmosLength;
							Vector3 chordMidpoint = (lineStartExtention + lineEndExtention) / 2f;
							Vector3 controlPoint = (4f * bezierCenterTarget - chordMidpoint) / 3f;
							Handles.DrawBezier(
								lineStartExtention,
								lineEndExtention,
								controlPoint,
								controlPoint,
								Handles.color,
								null,
								2f
							);
						#endif
					}
					
					return;
			}
		}
	}
}