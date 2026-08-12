using UnityEngine;
[System.Serializable]
public class LaserMod : Mod
{
	[SerializeField] private Sprite laserSprite;
	[SerializeField] private float laserLength;
	[SerializeField] private float laserWidth;
	[SerializeField] private float extendDuration;
	private float elapsedTime;
	private SpriteRenderer projectileSpriteRenderer;
	public override void Begin(Projectile projectile)
	{
		base.Begin(projectile);
		projectileSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
		projectileSpriteRenderer.sprite = laserSprite;
		projectileSpriteRenderer.drawMode = SpriteDrawMode.Tiled;
		projectile.transform.localScale = Vector3.one;

	}
	public override void End()
	{
	}

	public override void Run()
	{
		if (elapsedTime < extendDuration)
		{
			projectileSpriteRenderer.size = new Vector2(laserLength * Mathf.Min(1.0f, elapsedTime / extendDuration), laserWidth);
			elapsedTime += Time.deltaTime;
		}
	}
}
