using Sandbox;
using System;

public sealed class TestItem : BaseInteractive
{
	[Broadcast]
	public override void OnInteract( Guid userId )
	{
		Log.Info( $"Object: {GameObject.Name} has been destroyed" );

		GameObject.Destroy();
	}
}
