using Sandbox;

public sealed class AirTrigger : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		Log.Info( other.GameObject.Name );
	}

	public void OnTriggerExit( Collider other )
	{
		Log.Info( "Collider has leaved trigger" );
	}
}
