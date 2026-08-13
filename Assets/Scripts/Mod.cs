using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Mod
{
	protected Projectile projectile;

	public virtual void Begin(Projectile projectile) {this.projectile = projectile;}
	public abstract void Run();
	public virtual void End() {return;}

	public virtual void DrawGizmos() {return;}
}
