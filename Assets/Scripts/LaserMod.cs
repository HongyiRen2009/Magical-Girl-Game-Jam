using UnityEngine;
[System.Serializable]
public class LaserMod : Mod
{
	[SerializeField] private Sprite laserSprite;
	[SerializeField] private float laserLength = 4f;
	[SerializeField] private float laserWidth = 0.1f;
	[SerializeField] private float extendDuration = 0;
	[SerializeField] private float rotationAmount = 0;
	[SerializeField] private float rotationDuration = 0;
	private float elapsedTime;
	private float originalRotation;
	private SpriteRenderer projectileSpriteRenderer;
	public override void Begin(Projectile projectile)
	{
		base.Begin(projectile);
		projectileSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
		projectileSpriteRenderer.sprite = laserSprite;
		originalRotation = projectile.transform.eulerAngles.z;
		projectileSpriteRenderer.drawMode = SpriteDrawMode.Tiled;
		projectile.transform.localScale = Vector3.one;

	}

	public override float GetTravelDistance(float lifeTime)
	{
		return laserLength;
	}
	public override void End()
	{
	}

	public override void Run()
	{
		if (elapsedTime < extendDuration)
		{
			projectileSpriteRenderer.size = new Vector2(laserLength * Mathf.Min(1.0f, elapsedTime / extendDuration), laserWidth);
		}
		else if (elapsedTime < extendDuration + rotationDuration){
			float rotationProgress = (elapsedTime - extendDuration) / rotationDuration;
			projectile.transform.rotation = Quaternion.Euler(0, 0, originalRotation+rotationAmount * rotationProgress);
		}
		elapsedTime += Time.deltaTime;

	}
}
