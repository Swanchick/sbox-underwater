using Sandbox;
using Sandbox.Citizen;
using System;

public enum PlayerStates
{
	Walk,
	Run,
	Crouch,
	Swim
}

public sealed class Player : Component
{
	// Movement properties
	[Property] public float playerSpeed = 500f;
	[Property] public float groundFriction = 9f;
	[Property] public float airFriction = 0.3f;
	[Property] public float jumpForce = 300f;

	// Camera rotation properties
	[Property] public float cameraSensetivity = 5f;

	[Property] public CameraComponent playerCamera;
	[Property] public GameObject playerHead;
	[Property] public GameObject playerBody;

	private CharacterController playerController { get; set; }
	private CitizenAnimationHelper animationHelper { get; set; }

	private float sceneGravity;

	[Sync] public Rotation bodyRotation { get; set; }
	[Sync] public Vector3 wishDir { get; set; }

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
		
		sceneGravity = Scene.PhysicsWorld.Gravity.z;
	}

	protected override void OnUpdate()
	{
		CameraRotation();
		PlayerAnimation();
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

		Vector3 finalVelocity = BuildInput();
		finalVelocity = finalVelocity.WithZ( 0 );
		finalVelocity *= playerSpeed;

		if (playerController.IsOnGround)
		{
			// Applying friction
			playerController.ApplyFriction( groundFriction );

			playerController.Velocity = playerController.Velocity.WithZ( 0 );

			if ( Input.Pressed( "Jump" ) )
			{
				playerController.Punch( Vector3.Up * jumpForce );
				animationHelper.TriggerJump();
			}
		}
		else
		{
			// If the player is not on the ground, apply gravitation on the player
			playerController.Velocity += Vector3.Up * sceneGravity * Time.Delta;
			
			// Decreasing player's velocity if the player is not on the ground
			finalVelocity *= airFriction;
		}

		// Synchronizing velocity between all clients 
		wishDir = finalVelocity;

		// Applying velocity and move
		playerController.Accelerate( finalVelocity );
		playerController.Move();
	}
	
	// Rotating player camera
	private void CameraRotation()
	{
		// Applying rotation of body on all clients
		playerBody.Transform.Rotation = bodyRotation;
		
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

	private void PlayerAnimation()
	{
		animationHelper.WithWishVelocity( wishDir );
		animationHelper.WithVelocity( playerController.Velocity );
		animationHelper.IsGrounded = playerController.IsOnGround;
		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
	}
}
