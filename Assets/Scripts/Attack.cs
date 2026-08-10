using UnityEngine;

public class Attack : MonoBehaviour
{
	// projectile attributes
	[SerializeField] Vector3 position; // the bullet starting position...
	[SerializeField] float angle; // angle in degrees
	[SerializeField] float speed; // speed
	[SerializeField] float acceleration; // acceleration
	[SerializeField] float lifetime; // lifetime

	// spawning variables
	[SerializeField] float duration; // the time in seconds in which bullets will be spawned
	[SerializeField] float delay; // the time in seconds between individual bullets being spawned
	[SerializeField] Burst[] bursts; // bullet bursts that spawn multiple bullets in an instant
	public class Burst
	{
		[SerializeField] int projectileNumb; // the number of projectiles in the burst
		[SerializeField] float delay; // the delay in seconds that the burst occurs after the attack starts
		[SerializeField] bool spawnsEvenly; // determines if the bullets spawned will spawn evenly across any spacing random-ness present in the attack
	}
	[SerializeField] float spread; // the degree offset (positive or negitive) by which the bullet can deviate from its starting angle
	[SerializeField] float distanceVariability; // the position offset (positive or negitive) relitive to the angle which the bullet can spawn at

	float age; // the time that the attack has been occuring for

	[SerializeField] bool display = true;

    public void ExecuteAttack()
	{
		// will execute the attack
	}

    void OnDrawGizmos()
    {
        if (!display)
		{
			return;
		}

		Gizmos.color = Color.coral;
		Gizmos.DrawWireSphere(position, 0.5f);

		float degAngle = angle * Mathf.Deg2Rad;
		Vector3 target = new Vector3(Mathf.Cos(degAngle), Mathf.Sin(degAngle), 0);
		Gizmos.DrawLine(position, position + target);
    }
}
