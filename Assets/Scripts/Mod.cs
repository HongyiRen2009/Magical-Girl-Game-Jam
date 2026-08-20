using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class Mod
{
	protected Projectile projectile;
	public virtual object Clone() {return this.MemberwiseClone();}

	public virtual float GetTravelDistance(float lifeTime) { return 0; }

	public virtual void Begin(Projectile projectile) {this.projectile = projectile;}
	public virtual void Run() {return;}
	public virtual void End() {return;}

	public virtual void OnTransformParentChanged() {return;}

	public virtual void DrawGizmos(GameObject modded) {return;}
}
