using Sandbox;

public sealed class TestItem : BaseInteractive
{
	public override void OnInteract( Player player )
	{
		Log.Info( $"Object: {GameObject.Name} has been destroyed" );

		GameObject.Destroy();
	}
}
