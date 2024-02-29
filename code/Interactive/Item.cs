using Sandbox;
using System;

public class Item : BaseInteractive
{
	[Property] public string ImagePath { get; set; } = "/Textures/Items/iron.png";

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

	private void OnTake()
	{
		rigidBody.Gravity = false;
		collider.Enabled = false;
		modelRenderer.Enabled = false;
	}

	public void Drop()
	{
		
	}

	[Broadcast]
	public override void OnInteract( Guid playerId )
	{
		GameObject playerObject = FindPlayer( playerId );
		PlayerInventory inventory = playerObject.Components.Get<PlayerInventory>();

		OnTake();

		IsInteractive = false;
		inventory.Take( this );
	}
}
