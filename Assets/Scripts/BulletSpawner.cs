using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public float speed = 1f;
    public float lifeTime = 1f;
    public float size = 1f;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * speed * lifeTime);
        Gizmos.DrawWireSphere(transform.position, size);
    }
}
