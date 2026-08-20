using System.Collections;
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
    private TelegraphMod telegraphMod;

    static float despawnDistance = 20;
    static float sqrDespawnDistance => despawnDistance * despawnDistance;
    Camera cameraMain;

    public void Start()
    {
        cameraMain = Camera.main;
    }

    public void Initialize(float lifetime = 0, Mod[] mods = null)
    {
        this.lifetime = lifetime;
        this.mods = mods;

        StartCoroutine(StartMods());
	}

    private IEnumerator StartMods()
	{
		foreach (Mod mod in mods)
		{
			if (mod is TelegraphMod){
                mod.Begin(this);
                telegraphMod = (TelegraphMod)mod;
				yield return new WaitForSeconds(telegraphMod.telegraphDuration);
                break;
            }
		}
		// mods
        foreach (Mod mod in mods)
		{
			mod.Begin(this);
		}
	}

    void FixedUpdate()
    {
		if (telegraphMod != null)
		{
            telegraphMod.Run();
		}

        // lifetime check
        age += Time.fixedDeltaTime;
        if (lifetime > 0 && age > lifetime)
        {
            Despawn();
        }

        if ((cameraMain.transform.position - transform.position).sqrMagnitude > sqrDespawnDistance)
        {
            Despawn();
        }

        // mods
        foreach (Mod mod in mods)
        {
            mod.Run();
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
            mod.DrawGizmos(gameObject);
        }
    }
}