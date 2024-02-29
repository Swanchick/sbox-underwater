using Sandbox;
using System;

public class Item : BaseInteractive
{
	[Property] public string ImagePath { get; set; } = "/Textures/Items/iron.png";

	protected virtual bool IsInInventory { get; set; } = false;
	public bool IsCurrentItem { get; set; } = false;

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
		collider.Enabled = pleaseDontMake;
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

		inventory.Take( this );
	}

	protected override void OnUpdate()
	{
		Controlls();
	}

	protected virtual void Controlls()
	{
		if ( IsCurrentItem )
			return;
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
	}

	[Broadcast]
	public override void OnInteract( Guid playerId )
	{
		OnTake(playerId);
	}
}
