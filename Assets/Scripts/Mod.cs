using Unity.VisualScripting;
using UnityEngine;

public abstract class Mod : ScriptableObject
{
	public abstract void Begin(Projectile projectile);
	public abstract void Run(Projectile projectile);
	public abstract void End(Projectile projectile);
}
