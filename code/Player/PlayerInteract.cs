using Sandbox;

public sealed class PlayerInteract : Component
{
	[Property] public float interactDistance = 50f;
	[Property] public GameObject playerCamera;

	private PlayerManager playerManager;

	private SoundPointComponent playerInteractSound;

	protected override void OnStart()
	{
		playerInteractSound = Components.Get<SoundPointComponent>();
		playerManager = Components.Get<PlayerManager>();
	}

	protected override void OnUpdate()
	{
		Interact();
	}

	private void Interact()
	{
		if ( IsProxy )
			return;

		if ( !Input.Pressed( "use" ) )
			return;

		Vector3 cameraPosition = playerCamera.Transform.Position;
		Rotation cameraRotation = playerCamera.Transform.Rotation;

		SceneTraceResult trace = Scene.Trace
			.Ray( cameraPosition, cameraPosition + cameraRotation.Forward * interactDistance )
			.Run();

		if ( !trace.Hit )
		{
			return;
		}

		playerInteractSound.StopSound();
		playerInteractSound.StartSound();

		GameObject gameObject = trace.GameObject;
		IInteractive interactive = gameObject.Components.Get<IInteractive>();

		if ( interactive is null )
			return;

		if ( !interactive.IsInteractive )
			return;

		interactive.OnInteract( GameObject.Id );
	}
}
