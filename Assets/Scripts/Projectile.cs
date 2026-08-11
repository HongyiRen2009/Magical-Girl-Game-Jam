using UnityEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 1; // speed determines the speed that the bullets travel
    [SerializeField] float acceleration = 0; // the rate at which the speed increases
    [SerializeField] float lifetime = 0; // the time in seconds that the bullets last. If it is <= 0 than the bullet will not expire
    [SerializeField] bool visible = true;

    [SerializeField] Mod[] mods; // any mods which will be applyed to the bullet

    static float offscreenDespawnDistance = 10f; // determines the distance from the center of the screen that all bullets will despawn at

    float age = 0; // the time that the bullet has been alive for

    public void Start()
    {
        Debug.Log("ran start");
        Debug.Log(mods[0]);
        // mods
        foreach (Mod mod in mods)
        {
            Debug.Log("running the begin func");
            mod.Begin(this);
        }

        GetComponent<SpriteRenderer>().enabled = visible;
    }

    public void Initialize(float speed, float acceleration, float lifetime = 0, bool visible = true)
    {
        this.speed = speed;
        this.acceleration = acceleration;
        this.lifetime = lifetime;

        this.visible = visible;
        GetComponent<SpriteRenderer>().enabled = visible;
    }
    void FixedUpdate()
    {
        // movement
        gameObject.transform.position += transform.right * speed * Time.fixedDeltaTime;
        speed += acceleration;

        // offscreen check
        CheckIfOffscreen();

        // lifetime check
        age += Time.fixedDeltaTime;
        if (lifetime > 0 && age > lifetime)
        {
            Despawn();
        }

        // mods
        foreach (Mod mod in mods)
        {
            Debug.Log("running mod run");
            mod.Run(this);
        }
    }

    void CheckIfOffscreen()
    {
        /* 
            this is not the correct equation to determine distance from the center, but it works well enough and saves on processing power
            it also assumes that the camera is at 0, 0, which shouldnt be an issue...?
        */ 
        // This doesn't work right now
        if (Mathf.Abs(transform.position.x) + Mathf.Abs(transform.position.y) > offscreenDespawnDistance)
        {
           Despawn();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("hitSomthing");
        if (collision.tag == "Player" && visible)
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
            mod.End(this);
        }

        // later on we should create a manager to manage object pooling, because we really shouldnt be instantiateing and destroying so many bullets. Its not great on performance...
        Destroy(gameObject);
    }
}
