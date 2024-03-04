using Sandbox;
using System;

public class PlayerInventory : Component
{
	[Property] public GameObject inventoryStorage;
	[Property] public CameraComponent playerCamera;

	[Property] public float ThrowPower = 250f;

	public NetDictionary<int, Item> Inventory { get; set; } = new();
	public int MaxSlots { get; set; } = 4;

	public Action<NetDictionary<int, Item>> OnItemAdded;
	public Action<int> OnSlotChanged;

	private float currentSlot = 0;
	
	public bool CanTake()
	{
		Item item;
		Inventory.TryGetValue( (int)currentSlot, out item );

		return Inventory.Count <= MaxSlots && item is null;
	}

	public void Take(Item item, GameObject itemObject)
	{
		itemObject.Network.TakeOwnership();

		int slot = (int)currentSlot;

		Inventory[slot] = item;
		item.CurrentSlot = slot;

		OnItemAdded?.Invoke( Inventory );

		if ((int)currentSlot == Inventory.Count - 1 )
		{
			SetCurrentItem( (int)currentSlot );
		}
	}

	public void Drop()
	{
		Item item = GetItem( (int)currentSlot );
		if ( item is null )
			return;

		item.CurrentSlot = -100;
		Inventory.Remove( (int)currentSlot );
		OnItemAdded?.Invoke( Inventory );

		item.Drop( playerCamera.Transform.Position, playerCamera.Transform.Rotation.Forward, ThrowPower );


		GameObject itemObject = item.GameObject;
		itemObject.Network.DropOwnership();
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

		if ( Input.Pressed( "attack1" ) )
		{
			PrimaryFire();
		}

		if ( Input.Down( "attack1" ) )
		{
			PrimaryContinuousFire();
		}

		if ( Input.Pressed( "attack2" ) )
		{
			SecondaryFire();
		}
	}

	private BaseWeapon GetWeapon( int slot )
	{
		foreach (Item item in Inventory.Values )
		{
			if ( item.CurrentSlot != slot )
				continue;
			
			if (item is BaseWeapon )
			{
				return (BaseWeapon)item;
			}
		}

		return null;
	}

	private void PrimaryFire()
	{
		BaseWeapon weapon = GetWeapon( (int)currentSlot );
		if ( weapon is null )
			return;

		weapon.PrimaryFire();
	}

	private void PrimaryContinuousFire()
	{
		BaseWeapon weapon = GetWeapon( (int)currentSlot );
		if ( weapon is null )
			return;

		weapon.PrimaryContinuousFire();
	}

	private void SecondaryFire()
	{
		BaseWeapon weapon = GetWeapon( (int)currentSlot );
		if ( weapon is null )
			return;

		weapon.SecondaryFire();
	}

	private void SetCurrentItem(int slot)
	{
		foreach (Item _item in Inventory.Values )
		{
			_item.MakeDeactivateItem();
		}

		Item item = GetItem(slot);
		if ( item is null )
			return;

		item.MakeActivateItem();
	}
	
	private Item GetItem(int slot)
	{
		foreach ( Item item in Inventory.Values )
		{
			if (item.CurrentSlot == slot)
				return item;
		}

		return null;
	}

	private void ChangeSlot()
	{
		if ( IsProxy )
			return;

		float newSlot = currentSlot;

		newSlot -= Input.MouseWheel.y * 0.6f;
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
				currentSlot = slot;
				SetCurrentItem( slot );
				OnSlotChanged?.Invoke(slot);
			}
		}
	}
}
