using Sandbox;
using System;

public class PlayerInventory : Component
{
	[Property] public GameObject inventoryStorage;
	[Property] public CameraComponent playerCamera;

	[Property] public float ThrowPower = 250f;

	public List<Item> Inventory { get; set; } = new();
	public int MaxSlots { get; set; } = 4;

	public Action<List<Item>> OnItemAdded;
	public Action<int> OnSlotChanged;

	private float currentSlot = 0;
	
	public virtual void Take(Item item)
	{
		GameObject itemObject = item.GameObject;
		itemObject.Network.TakeOwnership();
		itemObject.Parent = inventoryStorage;
		itemObject.Transform.LocalPosition = Vector3.Zero;

		Inventory.Add(item);
		OnItemAdded?.Invoke( Inventory );

		if ((int)currentSlot == Inventory.Count - 1 )
		{
			SetCurrentItem( (int)currentSlot );
		}
	}

	public virtual void Drop()
	{
		Item item = GetItem( (int)currentSlot );
		if ( item is null )
			return;

		Inventory.Remove( item );
		OnItemAdded?.Invoke( Inventory );

		item.Drop(playerCamera.Transform.Rotation.Forward, ThrowPower);

		GameObject itemObject = item.GameObject;
		itemObject.SetParent( Scene );
		itemObject.Transform.Position = playerCamera.Transform.Position;
	}

	protected override void OnUpdate()
	{
		ChangeSlot();
		ChangeSlotByKeyboards();
		Controlls();
	}
	
	private void Controlls()
	{
		if ( IsProxy )
			return;

		if ( Input.Pressed("Drop") )
		{
			Drop();
		}
	}

	private void SetCurrentItem(int slot)
	{
		foreach (Item _item in Inventory )
		{
			_item.IsCurrentItem = false;
		}

		if ( slot > Inventory.Count - 1 )
			return;

		Item item = Inventory[slot];
		item.IsCurrentItem = true;

		Log.Info( "Item changed" );
	}
	
	private Item GetItem(int slot)
	{
		if ( slot > Inventory.Count - 1 )
			return null;

		return Inventory[slot];
	}

	private void ChangeSlot()
	{
		if ( IsProxy )
			return;

		float newSlot = currentSlot;

		newSlot -= Input.MouseWheel.y * 0.5f;
		newSlot = Math.Clamp( newSlot, 0, MaxSlots - 1 );

		if ( newSlot != currentSlot )
		{
			SetCurrentItem( (int)newSlot );
			OnSlotChanged?.Invoke( (int)newSlot );
		}

		currentSlot = newSlot;
	}

	private void ChangeSlotByKeyboards()
	{
		if ( IsProxy )
			return;

		for (int slot = 0; slot < MaxSlots; slot++ )
		{	
			if (Input.Pressed($"Slot{slot + 1}" ))
			{
				SetCurrentItem( slot );
				OnSlotChanged?.Invoke(slot);
			}
		}
	}
}
