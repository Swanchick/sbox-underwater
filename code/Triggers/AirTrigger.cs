using Sandbox;

public sealed class AirTrigger : Component, Component.ITriggerListener
{
	[Property] public bool canParentPlayer = false;
	[Property] public GameObject toParent;

	public void OnTriggerEnter( Collider other )
	{
		Player player = other.GameObject.Components.Get<Player>();

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
		Player player = other.GameObject.Components.Get<Player>();

		if ( player is null )
			return;

		player.LeavedAirTrigger( GameObject );
	}
}
