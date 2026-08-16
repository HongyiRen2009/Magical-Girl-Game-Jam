using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[System.Serializable]
public class TelegraphMod : Mod
{
	[Header("Telegraph")]
	[SerializeField] GameObject telegraphPrefab; // the prefab of the telegraph that will be spawned
	[SerializeField] private Sprite telegraphSprite; // the sprite of the telegraph that will be spawned
	public float telegraphDuration = 1f; // the duration of the telegraph
	[SerializeField] private Vector2 telegraphScale = new Vector2(1f, 1f);
	private float telegraphBlinkTimer;
	public override void Begin(Projectile projectile)
	{
		base.Begin(projectile);
		if (telegraphPrefab != null && telegraphDuration > 0f)
		{
			GameObject telegraph = UnityEngine.Object.Instantiate(telegraphPrefab, projectile.transform.position, projectile.transform.rotation);
			telegraph.transform.localScale = new Vector3(telegraphScale.x, telegraphScale.y, 1f);
			telegraph.GetComponent<SpriteRenderer>().sprite = telegraphSprite;
			UnityEngine.Object.Destroy(telegraph, telegraphDuration);
		}
	}
	public override void Run()
	{
		if(telegraphBlinkTimer >=0.1){
			projectile.GetComponent<SpriteRenderer>().enabled = !projectile.GetComponent<SpriteRenderer>().enabled;
			telegraphBlinkTimer = 0f;
		}
		else
		{
			telegraphBlinkTimer += Time.deltaTime;
		}
	}
}
