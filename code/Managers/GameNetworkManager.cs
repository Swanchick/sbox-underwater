using Sandbox;
using Sandbox.Network;
using System.Threading.Tasks;
using System;

[Title( "Game Network Manager" )]
[Category( "Managers" )]
[Icon( "electrical_services" )]
public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; }

	protected override void OnStart()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}

		base.OnStart();
	}

	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( PlayerPrefab is null )
			return;

		Transform startLocation = FindSpawnLocation().WithScale( 1 );

		GameObject player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );
		player.NetworkSpawn( channel );
	}

	private Transform FindSpawnLocation()
	{

		if ( SpawnPoints is not null && SpawnPoints.Count > 0 )
		{
			return Random.Shared.FromList( SpawnPoints, default ).Transform.World;
		}

		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}
		
		return Transform.World;
	}
}
