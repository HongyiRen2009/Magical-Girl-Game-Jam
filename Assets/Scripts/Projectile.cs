using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 1; // speed determines the speed that the bullets travel
    [SerializeField] float acceleration = 0; // the rate at which the speed increases
    [SerializeField] float lifetime = 0; // the time in seconds that the bullets last. If it is <= 0 than the bullet will not expire

    static float offscreenDespawnDistance = 10f; // determines the distance from the center of the screen that all bullets will despawn at

    float age = 0; // the time that the bullet has been alive for

    void Start()
    {
    }
    public void Initialize(float speed, float acceleration, float lifetime){
        this.speed = speed;
        this.acceleration = acceleration;
        this.lifetime = lifetime;

    }
    void FixedUpdate()
    {

        gameObject.transform.position += transform.right * speed * Time.fixedDeltaTime;

        speed += acceleration;


        CheckIfOffscreen();

        age += Time.fixedDeltaTime;
        if (lifetime > 0 && age > lifetime)
        {
            Despawn();
        }
    }

    void CheckIfOffscreen()
    {
        /* 
            this is not the correct equation to determine distance from the center, but it works well enough and saves on processing power
            it also assumes that the camera is at 0, 0, which shouldnt be an issue...?
        */ 
        // This doesn't work right now
        //if (transform.position.x + transform.position.y > offscreenDespawnDistance)
        //{
        //    Despawn();
        //}
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hitSomthing");
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerMovement>().Damaged(gameObject);

            Despawn();
        }
    }

    void Despawn()
    {
        // later on we should create a manager to manage object pooling, because we really shouldnt be instantiateing and destroying so many bullets. Its not great on performance...
        Destroy(gameObject);
    }
}
