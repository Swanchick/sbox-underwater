using Sandbox;

public sealed class AirTrigger : Component, Component.ITriggerListener
{
	[Property] public bool canParentPlayer = false;
	[Property] public GameObject toParent;

	public void OnTriggerEnter( Collider other )
	{
		PlayerMovement player = other.GameObject.Components.Get<PlayerMovement>();

		if ( player is null )
			return;

		if ( canParentPlayer )
		{
			player.EnteredIntoAirTrigger( GameObject, toParent );
		}
		else
		{
			player.EnteredIntoAirTrigger( GameObject );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		PlayerMovement player = other.GameObject.Components.Get<PlayerMovement>();

		if ( player is null )
			return;

		player.LeftAirTrigger( GameObject );
	}
}
