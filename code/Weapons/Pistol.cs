using Sandbox;

public class Pistol : BaseWeapon
{
	[Property] public SkinnedModelRenderer ViewModel;
	

	protected override void OnSlotActivate()
	{
		if ( IsProxy )
			return;

		ViewModel.GameObject.Enabled = true;
	}

	protected override void OnSlotDeactivate()
	{
		if ( IsProxy )
			return;

		ViewModel.GameObject.Enabled = false;

		
	}

	public override void PrimaryFire()
	{
		Log.Info( "Shoot!" );
		ViewModel.Set( "1H_Fire_01", true );
	}
}
