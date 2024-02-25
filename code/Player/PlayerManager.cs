using Sandbox;

public sealed class PlayerManager : Component
{
	private PlayerMovement playerMovement;
	private PlayerInteract playerInteract;
	private PlayerClothes playerClothes;

	protected override void OnStart()
	{
		playerMovement = Components.Get<PlayerMovement>();
		playerInteract = Components.Get<PlayerInteract>();
		playerClothes = Components.Get<PlayerClothes>();
	}
}
