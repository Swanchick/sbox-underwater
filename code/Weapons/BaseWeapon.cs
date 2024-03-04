using Sandbox;

public abstract class BaseWeapon : Item
{
	[Property] public virtual string WeaponName { get; set; } = "base";
	
	public virtual void PrimaryFire() { }
	public virtual void PrimaryContinuousFire() { }
	public virtual void SecondaryFire() { }

	public virtual void Reload() { }
}
