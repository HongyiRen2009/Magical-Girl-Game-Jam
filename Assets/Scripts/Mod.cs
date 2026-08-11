using Unity.VisualScripting;
using UnityEngine;

public abstract class Mod : ScriptableObject
{
	protected Projectile projectile;

	public virtual void Begin(Projectile projectile) {this.projectile = projectile;}
	public abstract void Run();
	public abstract void End();
}
