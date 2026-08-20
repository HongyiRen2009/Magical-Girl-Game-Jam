using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField] Attack attack;

	[SerializeField] bool drawGizmos;

    public void Activate()
    {
        attack.ExecuteAttack(gameObject);
    }

    void OnDrawGizmos()
    {
        if (drawGizmos) attack.DrawGizmos(gameObject);
    }

}
