using Sandbox;
using System.Threading.Tasks;

public sealed class PlayerManager : Component
{
	[Property] public GameObject PlayerHUD;

	private PlayerMovement playerMovement;
	private PlayerInteract playerInteract;
	private PlayerClothes playerClothes;
	private PlayerInventory playerInventory;

	protected override void OnStart()
	{
		playerMovement = Components.Get<PlayerMovement>();
		playerInteract = Components.Get<PlayerInteract>();
		playerClothes = Components.Get<PlayerClothes>();
		playerInventory = Components.Get<PlayerInventory>();
	
		if ( IsProxy )
		{
			PlayerHUD.Destroy();
		}

		PlayerHUD.Enabled = true;
	}
}
