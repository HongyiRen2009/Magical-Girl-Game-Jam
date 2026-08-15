using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    [SerializeField] float lifetime = 0; // the time in seconds that the bullets last. If it is <= 0 than the bullet will not expire
    float age = 0; // the time that the bullet has been alive for

    [SerializeReference] [SubclassSelector] Mod[] mods; // any mods which will be applyed to the bullet

    [SerializeField] bool active = true;

    [SerializeField] private bool parryable = false;

    public bool IsParryable => parryable;

    public void Start()
    {
        foreach (Mod mod in mods)
		{
			// Debug.Log("running the begin func");
			mod.Begin(this);
		}

        GetComponent<SpriteRenderer>().enabled = active;
    }

    public void Initialize(float lifetime = 0, Mod[] mods = null, bool active = true)
    {
        this.lifetime = lifetime;

        this.mods = mods;
		// mods
		foreach (Mod mod in mods)
		{
			// Debug.Log("running the begin func");
			mod.Begin(this);
		}

        this.active = active;
        GetComponent<SpriteRenderer>().enabled = active;
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
        if (active && collision.tag == "Player")
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

        while (transform.childCount > 0)
        {
            Debug.Log("releasing " + transform.GetChild(0));
            transform.GetChild(0).SetParent(transform.parent);
        }

        // later on we should create a manager to manage object pooling, because we really shouldnt be instantiateing and destroying so many bullets. Its not great on performance...
        Destroy(gameObject);
    }

    public void OnTransformParentChanged()
    {
        foreach (Mod mod in mods)
        {
            mod.OnTransformParentChanged();
        }
    }

    void OnDrawGizmos()
    {
        foreach (Mod mod in mods)
        {
            mod.DrawGizmos();
        }
    }
}