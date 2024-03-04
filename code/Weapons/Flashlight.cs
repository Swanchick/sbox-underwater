using Sandbox;

public class Flashlight : BaseWeapon
{
	[Property] public GameObject light;

	protected override void OnSlotDeactivate()
	{
		if ( IsProxy )
			return;

		light.Enabled = false;
	}

	public override void PrimaryFire()
	{
		light.Enabled = !light.Enabled;

		Log.Info( "Shoot!" );
	}
}
