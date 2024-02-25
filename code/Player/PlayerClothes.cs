using Sandbox;

public sealed class PlayerClothes : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer playerModelRenderer;
	[Property, TextArea] public string playerClothes;


	public void OnNetworkSpawn( Connection owner )
	{
		ClothingContainer clothing = new ClothingContainer();

		clothing.Deserialize( playerClothes );
		clothing.Apply( playerModelRenderer );
	}
}
