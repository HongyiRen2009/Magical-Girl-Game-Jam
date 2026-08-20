using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class M_Bullet : Mod
{
    [SerializeField] bool customBullet;

    [AllowNesting] [HideIf("customBullet")] [SerializeField] GameObject bulletPrefab;

    [AllowNesting] [ShowIf("customBullet")] [SerializeField] Sprite sprite;
    [AllowNesting] [ShowIf("customBullet")] [SerializeField] float hitboxRadius;
    [AllowNesting] [ShowIf("customBullet")] [SerializeField] float scale;

    [Header("Parry")]
    [SerializeField] public bool parryable;

    [Header("Active Timings")]
    [SerializeField] float delayActive;
    [SerializeField] float delayDisable;

    GameObject bullet;
    float time;

    public override void Begin(Projectile projectile)
    {
        base.Begin(projectile);

        if (customBullet)
        {
            bullet = new GameObject();

            CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
            collider.radius = hitboxRadius;
            collider.isTrigger = true;

            SpriteRenderer spriteRenderer = bullet.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            collider.transform.localScale = Vector3.one * scale;
            spriteRenderer.transform.localScale = Vector3.one * scale;
        }
        else
        {
            bullet = UnityEngine.Object.Instantiate(bulletPrefab);
        }

        bullet.transform.position = projectile.transform.position;
        bullet.transform.SetParent(projectile.transform);

        Bullet script = bullet.AddComponent<Bullet>();
        script.parryable = parryable;
        script.manager = this;
    }

    public override void Run()
    {
        base.Run();

        time += Time.fixedDeltaTime;
    }

    public bool IsActive()
    {
        return time > delayActive && (time < delayDisable || delayDisable <= 0);
    }

    public void Despawn()
    {
        projectile.Despawn();
    }

    public override void End()
    {
        base.End();

        UnityEngine.Object.Destroy(bullet);
    }

    public override void DrawGizmos(GameObject modded)
    {
        base.DrawGizmos(modded);

        if (customBullet)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(modded.transform.position, hitboxRadius * scale);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(modded.transform.position, sprite.bounds.size.x/2 * scale);
        }
    }
}

public class Hazard : MonoBehaviour
{
    public bool parryable = true;
}

public class Bullet : Hazard
{
    public M_Bullet manager;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (manager.IsActive() && collision.tag == "Player")
        {
            collision.GetComponent<PlayerMovement>().Damaged(this);

            manager.Despawn();
        }
    }
}
