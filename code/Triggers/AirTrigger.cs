using Sandbox;

public sealed class AirTrigger : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Player player = other.GameObject.Components.Get<Player>();

		if ( player is null )
			return;

		player.EnteredIntoAirTrigger( GameObject );
	}

	public void OnTriggerExit( Collider other )
	{
		Player player = other.GameObject.Components.Get<Player>();

		if ( player is null )
			return;

		player.LeavedAirTrigger( GameObject );
	}
}
