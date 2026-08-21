using NaughtyAttributes;
using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

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

[Serializable]
public class Attack
{

	[Header("Transform")]
	[SerializeField] public Vector3 positionOffset;
	[SerializeField] public float rotationOffset;
	[HideInInspector] public bool parentToModded;

	[Header("Bullet Settings")]
	[SerializeField] [Tooltip("The lifetime of the projectile. Set to 0 for infinite lifetime")] public float lifetime; // lifetime
	[SerializeReference] [SubclassSelector] public Mod[] mods; // the mods that will be applied to the projectile when it is spawned

	[Header("Spray")]
	[Tooltip("The time in seconds that projectiles are spawned for")] [SerializeField] public float duration;
	[Tooltip("The time in seconds between individual projectiles being spawned")] [SerializeField] public float firerate = 1f;
	[Tooltip("The time in seconds that elapses before projectiles spawn")] [SerializeField] public float delay;
	[Tooltip("The degree offset by which the projectile can deviate from its starting angle")] [SerializeField] public float spread;
	[AllowNesting] [ShowIf("spawnShape", SpawnShape.Line), Tooltip("Projectiles further away from the line's center will angle away from the lines center if true.")] [SerializeField] public bool useDeterministicSpread = true;

	[Header("Bursts")]
	[Tooltip("Spawn multiple bullets in an instant")] [SerializeField] public Burst[] bursts;

	[Header("Shape")]
	[Tooltip("The shape of the spawn area")] [SerializeField] public SpawnShape spawnShape;
	[ShowIf("spawnShape", SpawnShape.Line), Tooltip("How long the line that the bullets will spawn along is")] [SerializeField] public float spawnLineLength = 1; // the length of the line that the bullets will spawn along

	[Header("Gizmos")]
	[AllowNesting] [SerializeField] bool showSpray;
	[SerializeField] float gizmosLength = 2f;
	

	public void ExecuteAttack(GameObject modded)
	{
		AttackTimeline.current.StartCoroutine(ExecuteSpray(modded));

		foreach (Burst burst in bursts)
		{
			AttackTimeline.current.StartCoroutine(ExecuteBurst(modded, burst));
		}
	}

	public void ExecuteAttack(Vector3 position, Quaternion rotation)
	{
		AttackTimeline.current.StartCoroutine(ExecuteSpray(position, rotation));

		foreach (Burst burst in bursts)
		{
			AttackTimeline.current.StartCoroutine(ExecuteBurst(position, rotation, burst));
		}
	}

	// preforms a complete spray of bullets
	private IEnumerator ExecuteSpray(GameObject modded)
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(delay);

		float elapsed = 0; // time sense spray has started
		float lastSpawn = firerate; // time sense last projectile was spawned

		while (elapsed < duration)
		{
			// incriments the time
			elapsed += Time.deltaTime;
			lastSpawn += Time.deltaTime;

			if (lastSpawn > firerate)
			{
				Vector3 position = modded.transform.position + positionOffset;
				Quaternion rotation = modded.transform.rotation * Quaternion.Euler(0, 0, rotationOffset);

				GameObject proj = SpawnProjectile(position, rotation);

				if (parentToModded)
				{
					proj.transform.SetParent(modded.gameObject.transform);
				}

				lastSpawn -= firerate;
			}

			// waits for next frame
			yield return null;
		}
	}

	private IEnumerator ExecuteSpray(Vector3 position, Quaternion rotation)
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(delay);

		float elapsed = 0; // time sense spray has started
		float lastSpawn = firerate; // time sense last projectile was spawned

		while (elapsed < duration)
		{
			// incriments the time
			elapsed += Time.deltaTime;
			lastSpawn += Time.deltaTime;

			if (lastSpawn > firerate)
			{
				SpawnProjectile(position, rotation);

				lastSpawn -= firerate;
			}

			// waits for next frame
			yield return null;
		}
	}

	private IEnumerator ExecuteBurst(GameObject modded, Burst burst)
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(burst.delay);

		int iteration = 0;

		while (burst.repeat >= iteration)
		{
			iteration++;

			Vector3 position = modded.transform.position + positionOffset;
			Quaternion rotation = modded.transform.rotation * Quaternion.Euler(0, 0, rotationOffset);

			GameObject[] projs = SpawnBurst(position, rotation, burst);

			if (parentToModded)
			{
				foreach (GameObject proj in projs)
				{
					proj.transform.SetParent(modded.gameObject.transform);
				}
			}
			
			yield return new WaitForSeconds(burst.repeatDelay);
		}
	}

	private IEnumerator ExecuteBurst(Vector3 position, Quaternion rotation, Burst burst)
	{
		// awaits inital delay to complete
		yield return new WaitForSeconds(burst.delay);

		int iteration = 0;

		while (burst.repeat >= iteration)
		{
			iteration++;
			
			SpawnBurst(position, rotation, burst);
			
			yield return new WaitForSeconds(burst.repeatDelay);
		}
	}

	// spawns a projectile (baised on the initilized spray values)
	GameObject SpawnProjectile(Vector3 position, Quaternion rotation)
	{
		float randomAngleOffset = UnityEngine.Random.Range(-spread / 2, spread / 2);

		Vector3 spawnPosition = position;

		if (spawnShape == SpawnShape.Line)
		{
			float lineOffset = UnityEngine.Random.Range(-spawnLineLength / 2, spawnLineLength / 2);
			spawnPosition += rotation * Vector3.up * lineOffset; // could be a bug

			if (useDeterministicSpread)
			{
				randomAngleOffset = lineOffset / spawnLineLength * spread;
			}
		}

		Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 0, rotation.eulerAngles.z + randomAngleOffset));

		GameObject projectile = new GameObject();
		projectile.transform.position = spawnPosition;
		projectile.transform.rotation = spawnRotation;

		Projectile script = projectile.AddComponent<Projectile>();
		script.Initialize(lifetime, GetModsCopy());

		return projectile;
	}

	GameObject[] SpawnBurst(Vector3 position, Quaternion rotation, Burst burst)
	{
		GameObject[] ret = new GameObject[burst.projectileNum];

		for (int proj = 0; proj < burst.projectileNum; proj++)
		{
			float randomAngleOffset = UnityEngine.Random.Range(-burst.spread / 2, burst.spread / 2);

			if (burst.useDeterministicSpread)
			{
				float progression = proj/(burst.projectileNum-1f);
				progression -= 0.5f;

				randomAngleOffset = burst.spread * progression;
			}

			Vector3 spawnPosition = position;

			if (spawnShape == SpawnShape.Line)
			{
				float lineOffset = UnityEngine.Random.Range(-spawnLineLength / 2, spawnLineLength / 2);

				if (burst.useEvenSpawn)
				{
					float progression = proj/((float) burst.projectileNum-1);
					progression -= 0.5f;

					lineOffset = spawnLineLength * progression;
				}

				spawnPosition += rotation * Vector3.up * lineOffset;
			}

			Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 0, rotation.eulerAngles.z + randomAngleOffset));

			GameObject projectile = new GameObject();
			projectile.transform.position = spawnPosition;
			projectile.transform.rotation = spawnRotation;

			Projectile script = projectile.AddComponent<Projectile>();
			script.Initialize(lifetime, GetModsCopy());

			ret[proj] = projectile;
		}

		return ret;
	}

	Mod[] GetModsCopy()
	{
		Mod[] deepCopiedMods = new Mod[mods.Length];
		for (int i = 0; i < mods.Length; i++)
		{
			deepCopiedMods[i] = (Mod) mods[i].Clone();
		}
		return deepCopiedMods;
	}

    public void DrawGizmos(GameObject modded)
    {
		Vector3 position = modded.transform.position + positionOffset;
		Quaternion rotation = modded.transform.rotation * Quaternion.Euler(0, 0, rotationOffset);

		Gizmos.color = Color.red;
		Handles.color = Color.red;

		// drawing the attack shape
		switch(spawnShape)
		{
			case SpawnShape.Point:

				Gizmos.DrawWireSphere(position, 0.2f);
				
				break;

			case SpawnShape.Line:

				Vector3 up = rotation * Vector3.up;

				Vector3 lineStart = position - up * spawnLineLength / 2;
				Vector3 lineEnd = position + up * spawnLineLength / 2;
				Gizmos.DrawLine(lineStart, lineEnd);

				break;
		}

		foreach (Mod mod in mods)
		{
			if (mod != null)
			{
				mod.DrawGizmos(modded);
			}
		}

		// drawing spray
		if (showSpray)
		{
			DrawSpray(modded);
		}
		// Draw a cone for the spread of the burst or draw lines for each projectile in the burst if spreads evenly

		foreach (Burst burst in bursts)
		{
			if (burst.showGizmos)
			{
				DrawBurst(modded, burst);
			}
		}
	}

	void DrawSpray(GameObject modded)
	{
		Vector3 position = modded.transform.position + positionOffset;
		Quaternion rotation = modded.transform.rotation * Quaternion.Euler(0, 0, rotationOffset);

		Gizmos.color = Color.red;
		Vector3 vRot = rotation.eulerAngles * Mathf.Deg2Rad; // stands for vector rotation
		
		// if there is NO spread
		if (spread == 0)
		{
			switch (spawnShape)
			{
				// point, no spread
				case SpawnShape.Point:

					Vector3 Center = position + new Vector3(Mathf.Cos(vRot.z), Mathf.Sin(vRot.z), 0) * gizmosLength;
					Gizmos.DrawLine(position, Center);

					return;
				
				// line, no spread
				case SpawnShape.Line:

					float hLen = spawnLineLength / 2; // stands for half line length

					Vector3 up = rotation * Vector3.up;
					Vector3 right = rotation * Vector3.right;

					Vector3 lineStart = position - up * hLen;
					Vector3 lineEnd = position + up * hLen;

					Vector3 lineOffset = right * gizmosLength;

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

					Gizmos.DrawLine(position, position + FirstEnd);
					Gizmos.DrawLine(position, position + SecondEnd);

					#if UNITY_EDITOR
						Handles.DrawWireArc(position, Vector3.forward, FirstEnd, -spread, gizmosLength);
					#endif
					
					return;

				// line, yes spread
				case SpawnShape.Line:

					float hLen = spawnLineLength / 2; // stands for half line length

					Vector3 up = rotation * Vector3.up;
					Vector3 right = rotation * Vector3.right;

					Vector3 lineStart = position - up * hLen;
					Vector3 lineEnd = position + up * hLen;

					Vector3 lineStartExtention = lineStart + new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;
					Vector3 lineEndExtention = lineEnd + new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
					

					// Gizmos.DrawLine(lineStart, lineEnd);
					Gizmos.DrawLine(lineStart, lineStartExtention);
					Gizmos.DrawLine(lineEnd, lineEndExtention);
					
					#if UNITY_EDITOR
					Vector3 center = (lineEnd + lineStart) / 2;
					float baseAngleRad = rotation.eulerAngles.z * Mathf.Deg2Rad;
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

	void DrawBurst(GameObject modded, Burst burst)
	{
		Vector3 position = modded.transform.position + positionOffset;
		Quaternion rotation = modded.transform.rotation * Quaternion.Euler(0, 0, rotationOffset);

		Gizmos.color = Color.red;
		Vector3 vRot = rotation.eulerAngles * Mathf.Deg2Rad; // stands for vector rotation
		
		// if there is NO spread
		if (burst.spread == 0)
		{
			Vector3 Center = new Vector3(Mathf.Cos(vRot.z), Mathf.Sin(vRot.z), 0) * gizmosLength;

			switch (spawnShape)
			{
				// point, no spread
				case SpawnShape.Point:

					Gizmos.DrawLine(position, position + Center);

					for (int i = 0; i < burst.projectileNum; i++)
					{
						Gizmos.DrawSphere(position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
					}

					return;
				
				// line, no spread
				case SpawnShape.Line:

					Vector3 up = rotation * Vector3.up;
					Vector3 right = rotation * Vector3.right;
					
					Vector3 lineOffset = right * gizmosLength;

					if (burst.useEvenSpawn)
					{
						for (int i = 0; i < burst.projectileNum; i++)
						{
							float progression = i/((float) burst.projectileNum-1) - 0.5f;
							Vector3 linePosition = position + up * spawnLineLength * progression;

							Gizmos.DrawLine(linePosition, linePosition + lineOffset);
						}
					}
					else
					{
						float hLen = spawnLineLength / 2; // stands for half line length

						Vector3 lineStart = position - up * hLen;
						Vector3 lineEnd = position + up * hLen;

						Gizmos.DrawLine(lineStart, lineStart + lineOffset);
						Gizmos.DrawLine(lineEnd, lineEnd + lineOffset);
						Gizmos.DrawLine(lineStart + lineOffset, lineEnd + lineOffset);
						
						for (int i = 0; i < burst.projectileNum; i++)
						{
							Gizmos.DrawSphere(position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
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

							Gizmos.DrawLine(position, position + end);
						}
					}
					else
					{
						Vector3 FirstEnd = new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
						Vector3 SecondEnd = new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;

						Gizmos.DrawLine(position, position + FirstEnd);
						Gizmos.DrawLine(position, position + SecondEnd);

						#if UNITY_EDITOR
							Handles.DrawWireArc(position, Vector3.forward, FirstEnd, -burst.spread, gizmosLength);
						#endif

						for (int i = 0; i < burst.projectileNum; i++)
						{
							Gizmos.DrawSphere(position + Center * (i+1)/(burst.projectileNum+1), 0.05f);
						}
					}
					
					return;

				// line, yes spread
				case SpawnShape.Line:

					if (burst.useDeterministicSpread && burst.useEvenSpawn)
					{
						Vector3 up = rotation * Vector3.up;

						for (int i = 0; i < burst.projectileNum; i++)
						{
							float progression = i/((float) burst.projectileNum-1) - 0.5f;

							Vector3 linePosition = position + up * spawnLineLength * progression;

							float angle = progression * burst.spread * Mathf.Deg2Rad;
							Vector3 lineEnd = new Vector3(Mathf.Cos(vRot.z + angle), Mathf.Sin(vRot.z + angle), 0) * gizmosLength;

							Gizmos.DrawLine(linePosition, linePosition + lineEnd);
						}
					}
					else
					{
						float hLen = spawnLineLength / 2; // stands for half line length

						Vector3 up = rotation * Vector3.up;
						Vector3 right = rotation * Vector3.right;

						Vector3 lineStart = position - up * hLen;
						Vector3 lineEnd = position + up * hLen;

						Vector3 lineStartExtention = lineStart + new Vector3(Mathf.Cos(vRot.z - hSpr), Mathf.Sin(vRot.z - hSpr), 0) * gizmosLength;
						Vector3 lineEndExtention = lineEnd + new Vector3(Mathf.Cos(vRot.z + hSpr), Mathf.Sin(vRot.z + hSpr), 0) * gizmosLength;
						
						Gizmos.DrawLine(lineStart, lineStartExtention);
						Gizmos.DrawLine(lineEnd, lineEndExtention);
						
						#if UNITY_EDITOR
							Vector3 center = (lineEnd + lineStart) / 2;
							float baseAngleRad = rotation.eulerAngles.z * Mathf.Deg2Rad;
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