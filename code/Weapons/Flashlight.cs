using Sandbox;

public class Flashlight : BaseWeapon
{
	[Property] public GameObject light;

	private bool flashLightWorks = false;

	public override void PrimaryFire()
	{
		flashLightWorks = !flashLightWorks;

		light.Enabled = flashLightWorks;

		Log.Info( "Shoot!" );
	}
}
