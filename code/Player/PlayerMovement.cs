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

public sealed class PlayerMovement : Component, IAir
{
	// Movement properties
	[Property] public float playerWalkSpeed = 100f;
	[Property] public float playerRunSpeed = 250f;
	[Property] public float playerCrouchSpeed = 50f;
	[Property] public float crouchSpeed = 20f;
	[Property] public float playerWaterSpeed = 250f;

	[Property] public float runFriction = 5f;
	[Property] public float crouchFriction = 3f;
	[Property] public float airFriction = 0.3f;
	[Property] public float waterFriction = 5f;
	[Property] public float jumpForce = 300f;

	// Camera rotation properties
	[Property] public float cameraSensetivity = 0.1f;
	[Property] public float crouchHeight = 40f;

	[Property] public CameraComponent playerCamera;
	[Property] public GameObject playerHead;
	[Property] public GameObject playerBody;

	public Connection playerConnection;

	private CharacterController playerController { get; set; }
	private CitizenAnimationHelper animationHelper { get; set; }

	private float sceneGravity;
	private float defaultPlayerHeadHeight;
	private float defaultPlayerHeight;

	public List<GameObject> AirTrigger { get; private set; } = new();
	public bool InTheWater { get; set; } = true;

	[Sync] public Rotation bodyRotation { get; set; }
	[Sync] public Vector3 wishDir { get; set; }
	[Sync] public PlayerStates playerStates { get; set; }

	[Sync] public float currentSpeed { get; set; }
	[Sync] public float currentFriction { get; set; }
	[Sync] public CitizenAnimationHelper.MoveStyles currentMoveStyle { get; set; }
	List<GameObject> IAir.AirTrigger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	protected override void OnStart()
	{
		playerController = Components.Get<CharacterController>();
		animationHelper = Components.Get<CitizenAnimationHelper>();

		playerBody.Enabled = IsProxy;

		defaultPlayerHeadHeight = playerHead.Transform.LocalPosition.z;
		defaultPlayerHeight = playerController.Height;

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
		ControllPlayerStates();
	}

	protected override void OnFixedUpdate()
	{
		Move();
		MoveInWater();
	}

	private void ControllPlayerStates()
	{
		if ( IsProxy )
			return;

		if ( playerStates == PlayerStates.Swim )
			return;

		if ( Input.Down( "Run" ) && playerStates != PlayerStates.Crouch )
		{
			SetRun();
		} 
		else if ( Input.Down( "Duck" ) || CanUncrouch() )
		{
			SetCrouch();
		}
		else
		{
			SetWalk();
		}

		if (playerStates != PlayerStates.Crouch )
		{
			UncrouchHead();
		}
	}

	private void SetRun()
	{
		playerStates = PlayerStates.Run;
		currentSpeed = playerRunSpeed;
		currentMoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		currentFriction = runFriction;
	}

	private void SetWalk()
	{
		playerStates = PlayerStates.Walk;
		currentSpeed = playerWalkSpeed;
		currentMoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		currentFriction = runFriction;
	}

	private void SetCrouch()
	{
		playerStates = PlayerStates.Crouch;
		currentSpeed = 50f;
		currentMoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		currentFriction = crouchFriction;

		playerController.Height = crouchHeight;
		Vector3 headPosition = playerHead.Transform.LocalPosition;
		headPosition.z = crouchHeight - 7;

		playerHead.Transform.LocalPosition = Vector3.Lerp( playerHead.Transform.LocalPosition, headPosition, Time.Delta * crouchSpeed );
	}

	private void UncrouchHead()
	{
		playerController.Height = defaultPlayerHeight;
		Vector3 headPosition = playerHead.Transform.LocalPosition;
		headPosition.z = defaultPlayerHeadHeight;

		playerHead.Transform.LocalPosition = Vector3.Lerp( playerHead.Transform.LocalPosition, headPosition, Time.Delta * crouchSpeed );
	}

	private bool CanUncrouch()
	{
		Vector3 headPosition = playerHead.Transform.Position;

		float height = defaultPlayerHeight - crouchHeight - 7;

		BBox hull = new BBox( 0f, playerController.Radius + 5f );

		SceneTraceResult trace = Scene.Trace
			.Ray( headPosition, headPosition + Vector3.Up * height )
			.WithoutTags( "player", "item" )
			.Size( hull )
			.Run();

		return trace.Hit && playerStates == PlayerStates.Crouch;
	}

	public void OnAirEnter( GameObject trigger )
	{
		if ( AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Add( trigger );

		playerStates = PlayerStates.Walk;

		InTheWater = false;
	}

	public void OnAirEnterWithParent( GameObject trigger, GameObject parent )
	{
		OnAirEnter( trigger );

		GameObject.SetParent( parent, true );
	}

	public void OnAirLeave( GameObject trigger )
	{
		if ( !AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Remove( trigger );

		if ( AirTrigger.Count != 0 )
			return;

		GameObject.SetParent( Scene, true );

		playerStates = PlayerStates.Swim;

		InTheWater = true;
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
	
	// Building velocity from player input for water movement
	private Vector3 BuildVerticalInput()
	{
		float upDown = SUtils.GetButton( "Jump" ) - SUtils.GetButton( "Duck" );
		Vector3 direction = Vector3.Up * upDown;

		return direction.Normal;
	}

	// Creating velocity and accelerating a player there
	private void Move()
	{
		if ( IsProxy )
			return;

		if ( playerStates == PlayerStates.Swim )
			return;

		Vector3 finalVelocity = BuildInput();
		finalVelocity = finalVelocity.WithZ( 0 );
		finalVelocity *= currentSpeed;

		if (playerController.IsOnGround)
		{
			// Applying friction
			playerController.ApplyFriction( currentFriction );

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

	// Creating velocity and accelerating a player there in water
	private void MoveInWater()
	{
		if (IsProxy)
			return;

		if ( playerStates != PlayerStates.Swim )
			return;

		Vector3 finalVelocity = BuildInput();
		finalVelocity += BuildVerticalInput();
		finalVelocity = finalVelocity.Normal;
		finalVelocity *= playerWaterSpeed;

		wishDir = finalVelocity;

		// I don't know why, but player some how gets stucked in the floor, and cannot swim on top
		// So that's why added this :)
		if (playerController.IsOnGround && Input.Down( "Jump" ) )
		{
			playerController.Punch( Vector3.Up * currentSpeed );
		}

		playerController.ApplyFriction( waterFriction );
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
		animationHelper.AimAngle = playerCamera.Transform.Rotation;
		animationHelper.IsGrounded = playerController.IsOnGround && playerStates != PlayerStates.Swim;
		animationHelper.IsSwimming = playerStates == PlayerStates.Swim;
		animationHelper.MoveStyle = currentMoveStyle;

		animationHelper.DuckLevel = playerStates == PlayerStates.Crouch ? 70f : 0f;
	}
}
