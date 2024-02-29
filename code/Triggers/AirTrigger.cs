using Sandbox;

public sealed class AirTrigger : Component, Component.ITriggerListener
{
	[Property] public bool canParent = false;
	[Property] public GameObject toParent;

	public void OnTriggerEnter( Collider other )
	{
		IAir air = other.GameObject.Components.Get<IAir>();

		if ( air is null )
			return;

		if ( canParent )
		{
			air.OnAirEnterWithParent( GameObject, toParent );
		}
		else
		{
			air.OnAirEnter( GameObject );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		IAir air = other.GameObject.Components.Get<IAir>();

		if ( air is null )
			return;

		air.OnAirLeave( GameObject );
	}
}
