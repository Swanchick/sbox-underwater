using Sandbox;
using System;

public enum ItemState
{
	None,
	Inventory,
	Active
}

public class Item : BaseInteractive, IAir
{
	[Property] public string ImagePath { get; set; } = "/Textures/Items/iron.png";

	[Property] public float WaterFriction { get; set; } = 2f;
	[Property] public float WaveHieght { get; set; } = 10f;

	public ItemState CurrentItemState { get; private set; } = ItemState.None;

	[Sync] public bool inTheWater { get; set; } = false;

	public List<GameObject> AirTrigger { get; set; } = new();

	private GameObject objectToParent;
	
	private Rigidbody rigidbody;
	private ModelRenderer modelRenderer;

	protected override void OnStart()
	{
		rigidbody = Components.Get<Rigidbody>();
		modelRenderer = Components.Get<ModelRenderer>();

		Network.SetOwnerTransfer( OwnerTransfer.Takeover );
	}

	protected override void OnUpdate()
	{
		MoveInWater();
	}

	protected virtual void MoveInWater()
	{
		if ( !inTheWater )
			return;

		rigidbody.Velocity = Vector3.Lerp( rigidbody.Velocity, Vector3.Zero, Time.Delta * WaterFriction );
		rigidbody.Velocity = rigidbody.Velocity.WithZ(rigidbody.Velocity.z + (float)Math.Sin( Time.Now ) * WaveHieght * Time.Delta);
		rigidbody.AngularVelocity = Vector3.Lerp( rigidbody.AngularVelocity, Vector3.Zero, Time.Delta * WaterFriction );
	}

	protected virtual void MakeItemForSlot(bool pleaseDontMake)
	{
		rigidbody.Gravity = pleaseDontMake;
		modelRenderer.Enabled = pleaseDontMake;
	}

	public override void OnInteract( Guid playerId )
	{
		if ( CurrentItemState != ItemState.None )
			return;

		GameObject playerObject = FindPlayer( playerId );
		PlayerInventory inventory = playerObject.Components.Get<PlayerInventory>();

		OnTake( playerId );

		inventory.Take( this, GameObject );
	}

	[Broadcast]
	public virtual void MakeActivateItem()
	{
		CurrentItemState = ItemState.Active;

		OnActivate();
	}

	[Broadcast]
	public virtual void MakeDeactivateItem()
	{
		CurrentItemState = ItemState.Inventory;

		OnDeactivate();
	}

	protected virtual void OnActivate() { }

	protected virtual void OnDeactivate() { }

	[Broadcast]
	protected virtual void OnTake(Guid playerId)
	{
		GameObject playerObject = FindPlayer( playerId );

		MakeItemForSlot( false );

		IsInteractive = false;
		GameObject.SetParent( playerObject, false );
	}

	[Broadcast]
	public virtual void Drop(Vector3 cameraPos, Vector3 forward, float throwPower)
	{	
		if ( CurrentItemState == ItemState.None )
			return;

		CurrentItemState = ItemState.None;

		IsInteractive = true;
		
		MakeItemForSlot( true );

		Transform.Rotation = Rotation.Identity;

		GameObject.SetParent( objectToParent );
		Transform.Position = cameraPos;
		rigidbody.Velocity = forward * throwPower;

		rigidbody.Gravity = !inTheWater;
	}

	public void OnAirEnter( GameObject trigger )
	{
		if ( AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Add( trigger );

		inTheWater = false;

		if ( CurrentItemState != ItemState.None )
			return;

		if ( rigidbody is null )
			return;

		rigidbody.Gravity = true;
	}

	public void OnAirEnterWithParent( GameObject trigger, GameObject parent )
	{
		OnAirEnter( trigger );

		if (CurrentItemState != ItemState.None )
		{
			objectToParent = parent;

			return;
		}

		GameObject.SetParent( parent, true );
	}

	public void OnAirLeave( GameObject trigger )
	{
		if ( !AirTrigger.Contains( trigger ) )
			return;

		AirTrigger.Remove( trigger );

		if ( AirTrigger.Count != 0 )
			return;

		inTheWater = true;

		if ( CurrentItemState != ItemState.None )
			return;

		objectToParent = Scene;
		GameObject.SetParent( Scene, true );

		if ( rigidbody is null )
			return;

		rigidbody.Gravity = false;
	}
}
