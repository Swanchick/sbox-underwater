using Sandbox;
using Sandbox.Citizen;
using System;

public sealed class Player : Component
{
	// Movement properties
	[Property] public float playerSpeed = 500f;
	[Property] public float groundFriction = 9f;

	// Camera rotation properties
	[Property] public float cameraSensetivity = 5f;

	[Property] public CameraComponent playerCamera;
	[Property] public GameObject playerHead;
	[Property] public GameObject playerBody;

	private CharacterController playerController;
	private CitizenAnimationHelper animationHelper;

	[Sync] public Rotation bodyRotation { get; set; }

	protected override void OnStart()
	{
		playerController = Components.Get<CharacterController>();
		animationHelper = Components.Get<CitizenAnimationHelper>();

		playerBody.Enabled = IsProxy;

		// If the player is not yours, delete the camera
		if ( IsProxy )
		{
			playerCamera.Destroy();
			
			return;
		}
	
	}

	protected override void OnUpdate()
	{
		CameraRotation();
	}

	protected override void OnFixedUpdate()
	{
		Move();
	}

	// Building velocity from player input
	private Vector3 BuildInput()
	{
		// Getting there difference between buttons D and A
		float vertical = SUtils.GetButton( "forward" ) - SUtils.GetButton( "backward" );
		// Getting there difference between buttons W and S
		float horizontal = SUtils.GetButton( "right" ) - SUtils.GetButton( "left" );

		Rotation platerRotation = playerHead.Transform.Rotation;
		Vector3 direction = platerRotation.Forward * vertical + platerRotation.Right * horizontal;

		return direction.Normal;
	}
	
	// Creating velocity and accelerating a player there
	private void Move()
	{
		if ( IsProxy )
			return;

		Vector3 playerVelocity = BuildInput();
		playerVelocity = playerVelocity.WithZ( 0 );
		playerVelocity *= playerSpeed;

		
	}
	
	// Rotating player camera
	private void CameraRotation()
	{
		if ( IsProxy )
			return;

		// Getting input from client's mouse
		Vector2 mouseDelta = Input.MouseDelta;

		// Getting rotation from head and camera
		Angles headAngles = playerHead.Transform.LocalRotation.Angles();
		Angles cameraAngles = playerCamera.Transform.LocalRotation.Angles();
		
		// Rotating player head in horizontal
		headAngles.yaw -= mouseDelta.x * cameraSensetivity;
		
		// Rotating player camera in vertical
		float pitchAngle = cameraAngles.pitch + mouseDelta.y * cameraSensetivity;
		cameraAngles.pitch = Math.Clamp( pitchAngle, -89f, 89f );

		// Synchronizing angles between all clients on server
		bodyRotation = headAngles.ToRotation();

		// Applying rotation
		playerHead.Transform.LocalRotation = headAngles.ToRotation();
		playerCamera.Transform.LocalRotation = cameraAngles.ToRotation();
	}
}
