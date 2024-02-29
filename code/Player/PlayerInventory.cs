using Sandbox;

public class PlayerInventory : Component
{
	[Property] public GameObject inventoryStorage;

	public List<Item> Inventory { get; set; } = new();

	public void Take(Item item)
	{
		GameObject itemObject = item.GameObject;
		itemObject.Network.TakeOwnership();
		itemObject.Parent = inventoryStorage;
		itemObject.Transform.LocalPosition = Vector3.Zero;

		Inventory.Add(item);
	}
}
