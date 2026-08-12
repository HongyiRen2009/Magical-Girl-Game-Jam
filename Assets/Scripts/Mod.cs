using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Mod
{
	protected Projectile projectile;
	public object Clone()
	{
		return this.MemberwiseClone();
	}
	public virtual void Begin(Projectile projectile) {this.projectile = projectile;}
	public abstract void Run();
	public abstract void End();
}
