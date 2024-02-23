using Sandbox;
using Sandbox.Citizen;
using System.Numerics;

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
		if ( IsProxy )
			return;

		Move();
	}

	// Building velocity from player input
	private Vector3 BuildInput()
	{

		// Getting there difference between buttons D and A
		float vertical = SUtils.GetButton( "forward" ) - SUtils.GetButton( "backward" );
		// Getting there difference between buttons W and S
		float horizontal = SUtils.GetButton( "right" ) - SUtils.GetButton( "left" );

		Rotation platerRotation = Transform.Rotation;
		Vector3 direction = platerRotation.Forward * vertical + platerRotation.Right * horizontal;

		return direction.WithZ(0).Normal;
	}


	// Creating velocity and accelerating a player there
	private void Move()
	{
		Vector3 playerVelocity = BuildInput();
		playerVelocity *= playerSpeed;


		
	}
}
