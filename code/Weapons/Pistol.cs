using Sandbox;

public class Pistol : BaseWeapon
{
	[Property] public GameObject ViewModel;

	protected override void OnSlotActivate()
	{
		if ( IsProxy )
			return;

		ViewModel.Enabled = true;
	}

	protected override void OnSlotDeactivate()
	{
		if ( IsProxy )
			return;

		ViewModel.Enabled = false;
	}
}
