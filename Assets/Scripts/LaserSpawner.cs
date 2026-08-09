using System.Net;
using UnityEditor;
using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
	public float width = 1f;
	public float length = 5f;
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = Color.red;
		Handles.DrawLine(transform.position, transform.position + transform.right * length, width);
	}
#endif
}
