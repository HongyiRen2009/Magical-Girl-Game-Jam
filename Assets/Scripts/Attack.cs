using UnityEngine;

public abstract class Attack : MonoBehaviour
{
	public bool parryable = false;
	public virtual void OnParry() { }
	public abstract void ExecuteAttack();
}
