using UnityEngine;

[CreateAssetMenu(fileName = "LaserMod", menuName = "Bullet Mods/Laser Mod")]
public class M_Laser : Mod
{
    [SerializeField] Vector3 targetPosition; // the second position that the laser will draw itself to
    [SerializeField] float angle; // the angle by which the target will travel in
    [SerializeField] float speed; // the speed by which the target will move
    [SerializeField] float acceleration; // the acceleration of the target along the angle

    [SerializeField] Mod[] mods; // these are the mods which are attached to the target

    [SerializeField] float delay; // time (in seconds) in which the laser is delayed (in this time, some telegraph would be played)
    [SerializeField] float travelTime; // the time (in seconds) that it takes for the beam to reach and retract from the target
    [SerializeField] bool localTransform; // determines if the target position is a child of the position of the bullet or not

    [SerializeField] GameObject projectile;
    [SerializeField] Sprite sprite;
    [SerializeField] float width;

    GameObject laser;
    SpriteRenderer renderer;
    BoxCollider2D collider;
    Projectile targetProj;


    public override void Begin(Projectile projectile)
    {
        Debug.Log("begun");
        // creating the projectile that will repersent the target
        GameObject projObject = Instantiate(this.projectile, targetPosition, Quaternion.Euler(new Vector3(0, 0, angle)));

        targetProj = projObject.GetComponent<Projectile>();
        targetProj.Initialize(speed, acceleration, 0, false);

        // creating the laser object
        laser = new GameObject();

        // adding laser script
        laser.AddComponent<Laser>();

        // initilizing the laser renderer
        renderer = laser.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;

        // initilizing the lazer hitbox
        collider = laser.AddComponent<BoxCollider2D>();
        collider.size = Vector3.one * width;

        UpdateLaser(projectile);
    }
	public override void Run(Projectile projectile) 
    {
        UpdateLaser(projectile);
    }

    void UpdateLaser(Projectile projectile)
    {
        Debug.Log("updating laser");
        Debug.Log(renderer);
        Vector3 targetDirection = targetProj.transform.position - projectile.transform.position;

        Vector3 laserSize = new Vector3(width, targetDirection.magnitude);
        
        renderer.size = laserSize;
        collider.size = laserSize;

        laser.transform.position = projectile.transform.position + targetDirection/2;
        laser.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90 + Mathf.Atan2(targetDirection.y, targetDirection.x)));
    }
    public override void End(Projectile projectile) 
    {
        Destroy(laser);
        targetProj.Despawn();
    }
}

public class Laser : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("laser has hit the player");
    }
}
