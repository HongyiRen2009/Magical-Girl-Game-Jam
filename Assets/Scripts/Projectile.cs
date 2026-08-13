using UnityEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float lifetime = 0; // the time in seconds that the bullets last. If it is <= 0 than the bullet will not expire

    [SerializeReference] [SubclassSelector] Mod[] mods; // any mods which will be applyed to the bullet

    float age = 0; // the time that the bullet has been alive for

    public void Start()
    {
        foreach (Mod mod in mods)
		{
			// Debug.Log("running the begin func");
			mod.Begin(this);
		}
    }
    
    public void Initialize(float lifetime = 0, Mod[] mods = null)
    {
        this.lifetime = lifetime;

        this.mods = mods;
		// mods
		foreach (Mod mod in mods)
		{
			// Debug.Log("running the begin func");
			mod.Begin(this);
		}
	}

    void FixedUpdate()
    {
        // lifetime check
        age += Time.fixedDeltaTime;
        if (lifetime > 0 && age > lifetime)
        {
            Despawn();
        }

        // mods
        foreach (Mod mod in mods)
        {
            mod.Run();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerMovement>().Damaged(gameObject);

            Despawn();
        }
    }

    public void Despawn()
    {
        // mods
        foreach (Mod mod in mods)
        {
            mod.End();
        }

        // later on we should create a manager to manage object pooling, because we really shouldnt be instantiateing and destroying so many bullets. Its not great on performance...
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        foreach (Mod mod in mods)
        {
            mod.DrawGizmos();
        }
    }
}
