using UnityEngine;
[System.Serializable]
public class SplitMod : Mod
{
[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private int numberOfProjectiles = 2;
	[SerializeField] private float splitProjectileLifetime = 2f;
	[SerializeReference][SubclassSelector] private Mod[] splitProjectileMods; // the mods that will be applied to the projectile when it is spawned
	public override void Run()
	{
	}
	public override void End()
	{
		for (int i = 0; i < numberOfProjectiles; i++)
		{
			float angle = (360f / numberOfProjectiles) * i;
			GameObject newProjectile = GameObject.Instantiate(projectilePrefab, projectile.transform.position, Quaternion.Euler(0, 0, angle));
			Projectile newProjectileComponent = newProjectile.GetComponent<Projectile>();
			Mod[] deepCopiedMods = new Mod[splitProjectileMods.Length];
			for (int j = 0; j < splitProjectileMods.Length; j++)
			{
				deepCopiedMods[j] = (Mod)splitProjectileMods[j].Clone();
			}
			newProjectileComponent.Initialize(splitProjectileLifetime, deepCopiedMods, true);
		}
	}
}
