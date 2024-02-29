using Sandbox;
using System;

public class PlayerInventory : Component
{
	[Property] public GameObject inventoryStorage;

	public List<Item> Inventory { get; set; } = new();
	public int MaxSlots { get; set; } = 4;

	public Action<List<Item>> OnItemAdded;
	public Action<int> OnSlotChanged;

	private float currentSlot = 0;
	
	public void Take(Item item)
	{
		GameObject itemObject = item.GameObject;
		itemObject.Network.TakeOwnership();
		itemObject.Parent = inventoryStorage;
		itemObject.Transform.LocalPosition = Vector3.Zero;

		Inventory.Add(item);

		OnItemAdded?.Invoke( Inventory );
	}

	protected override void OnUpdate()
	{
		ChangeSlot();
	}

	private void ChangeSlot()
	{
		if ( IsProxy )
			return;

		float newSlot = currentSlot;

		newSlot += Input.MouseWheel.y * 0.5f;
		newSlot = Math.Clamp( newSlot, 0, MaxSlots - 1 );

		if ( newSlot != currentSlot )
		{
			Log.Info( "Slot has been changed" );

			OnSlotChanged?.Invoke((int)newSlot);
		}

		currentSlot = newSlot;
	}
}
