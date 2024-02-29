using Sandbox;
using System;
using System.Runtime.InteropServices;

public class Item : BaseInteractive, IAir
{
	[Property] public string ImagePath { get; set; } = "/Textures/Items/iron.png";

	protected virtual bool IsInInventory { get; set; } = false;
	public bool IsCurrentItem { get; set; } = false;
	public List<GameObject> AirTrigger { get; set; } = new();

	[Sync] public bool inTheWater { get; set; } = false;

	private Rigidbody rigidBody;
	private BoxCollider collider;
	private ModelRenderer modelRenderer;

	protected override void OnStart()
	{
		rigidBody = Components.Get<Rigidbody>();
		collider = Components.Get<BoxCollider>();
		modelRenderer = Components.Get<ModelRenderer>();

		Network.SetOwnerTransfer( OwnerTransfer.Takeover );
	}

	protected virtual void MakeItemForSlot(bool pleaseDontMake)
	{
		rigidBody.Gravity = pleaseDontMake;
		// collider.Enabled = pleaseDontMake;
		modelRenderer.Enabled = pleaseDontMake;
	}

	protected virtual void OnTake(Guid playerId)
	{
		if ( IsInInventory )
			return;
		
		GameObject playerObject = FindPlayer( playerId );
		PlayerInventory inventory = playerObject.Components.Get<PlayerInventory>();

		MakeItemForSlot(false);

		IsInInventory = true;
		IsInteractive = false;

		rigidBody.Velocity = Vector3.Zero;
		inventory.Take( this );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		MoveInWater();
	}

	protected virtual void MoveInWater()
	{
		if ( !inTheWater )
			return;

		rigidBody.Velocity = Vector3.Lerp( rigidBody.Velocity, Vector3.Zero, Time.Delta * 2f );
		rigidBody.AngularVelocity = Vector3.Lerp( rigidBody.AngularVelocity, Vector3.Zero, Time.Delta * 2f );

	}

	[Broadcast]
	public virtual void Drop(Vector3 forward, float throwPower)
	{	
		if ( !IsInInventory )
			return;

		IsInInventory = false;
		IsCurrentItem = false;

		IsInteractive = true;
		MakeItemForSlot( true );
		Network.DropOwnership();

		rigidBody.Velocity = forward * throwPower;

		rigidBody.Gravity = !inTheWater;
	}

	[Broadcast]
	public override void OnInteract( Guid playerId )
	{
		OnTake(playerId);
	}

	[Broadcast]
	public void OnAirEnter( GameObject trigger )
	{
		if ( IsInInventory )
			return;

		if ( AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Add( trigger );

		inTheWater = false;

		if ( IsInInventory )
			return;


		rigidBody.Gravity = true;
	}

	[Broadcast]
	public void OnAirEnterWithParent( GameObject trigger, GameObject parent )
	{
		OnAirEnter( trigger );

		if ( IsInInventory )
			return;

		GameObject.SetParent( parent, true );
	}

	[Broadcast]
	public void OnAirLeave( GameObject trigger )
	{
		if ( !AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Remove( trigger );

		if ( AirTrigger.Count != 0 )
			return;

		inTheWater = true;

		if ( IsInInventory )
			return;

		rigidBody.Gravity = false;

		GameObject.SetParent( null, true );
	}
}
