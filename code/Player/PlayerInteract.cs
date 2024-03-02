using Sandbox;
using System;

public sealed class PlayerInteract : Component
{
	[Property] public float interactDistance = 50f;
	[Property] public GameObject playerCamera;

	private PlayerManager playerManager;
	private SoundPointComponent playerInteractSound;

	public Action OnItemFound;
	public Action OnItemLost;

	private bool crosshairActive = false;

	protected override void OnStart()
	{
		playerInteractSound = Components.Get<SoundPointComponent>();
		playerManager = Components.Get<PlayerManager>();
	}

	protected override void OnUpdate()
	{
		Interact();
	}

	private void CrosshairState( bool hit, bool isInteractive )
	{
		if ( !hit )
		{
			if ( crosshairActive )
			{
				crosshairActive = false;

				OnItemLost?.Invoke();
			}

			return;
		}


		if ( isInteractive && !crosshairActive )
		{
			crosshairActive = true;

			OnItemFound?.Invoke();
		}
	}

	private void Interact()
	{
		if ( IsProxy )
			return;

		Vector3 cameraPosition = playerCamera.Transform.Position;
		Rotation cameraRotation = playerCamera.Transform.Rotation;

		SceneTraceResult trace = Scene.Trace
			.Ray( cameraPosition, cameraPosition + cameraRotation.Forward * interactDistance )
			.Run();

		if ( !trace.Hit )
		{
			CrosshairState( false, false );

			return;
		}

		GameObject gameObject = trace.GameObject;
		IInteractive interactive = gameObject.Components.Get<IInteractive>();

		CrosshairState( interactive is not null, false);

		if ( interactive is null )
			return;

		CrosshairState( true, interactive.IsInteractive );

		if ( !interactive.IsInteractive )
			return;

		if ( !Input.Pressed( "use" ) )
			return;

		playerInteractSound.StopSound();
		playerInteractSound.StartSound();

		interactive.OnInteract( GameObject.Id );
	}
}
