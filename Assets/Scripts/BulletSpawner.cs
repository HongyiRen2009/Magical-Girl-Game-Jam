using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletSpawner : Spawner
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
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = Color.red;
		Handles.DrawLine(transform.position, transform.position + transform.right * speed * lifeTime);
        Handles.DrawSolidDisc(transform.position, Vector3.forward, size);

	}
#endif
}
