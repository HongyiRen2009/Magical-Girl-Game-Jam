using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
[System.Serializable]
public class SplitMod : Mod
{
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private Mod[] splitMods;
	[SerializeField] private int numSplitProjectiles;
	[SerializeField] private bool uniformSplit;
	[SerializeField] private float splitLifetime;
	public override void End()
	{
		float currentAngle = 0;
		for(int i=0; i<numSplitProjectiles; i++){
			GameObject projectileGameObject = UnityEngine.Object.Instantiate(projectilePrefab, projectile.transform.position, Quaternion.Euler(new Vector3(0, 0, uniformSplit ? currentAngle:Random.Range(0,360))));
			projectileGameObject.GetComponent<Projectile>().Initialize(splitLifetime,splitMods);
			currentAngle += 360f / numSplitProjectiles;
		}
	}

	public override void Run()
	{
	}
}
