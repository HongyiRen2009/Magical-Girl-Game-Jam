using UnityEngine;

public class Oscillate : MonoBehaviour
{
    [SerializeField] private Vector2 firstPoint;
    [SerializeField] private Vector2 secondPoint;
	[SerializeField] private float speed = 1f;
    private float interpolateT;
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		interpolateT += Time.deltaTime * speed;
        transform.position = Vector2.Lerp(firstPoint, secondPoint, Mathf.PingPong(interpolateT, 1f));
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(firstPoint, 0.1f);
		Gizmos.DrawSphere(secondPoint, 0.1f);
        Gizmos.DrawLine(firstPoint, secondPoint);
	}
}
