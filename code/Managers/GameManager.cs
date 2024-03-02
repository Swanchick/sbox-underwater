using Sandbox;
using Sandbox.Menu;
using Sandbox.Network;
using System;
using System.Threading.Tasks;

[Title( "Game Manager" )]
[Category( "Managers" )]
[Icon( "electrical_services" )]
public sealed class GameManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject RedSpawnPoint { get; set; }

	[Sync] public bool IsRoundStarted { get; set; } = false;
	[Sync] public NetList<PlayerTeam> PlayersTeam { get; set; } = new();

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( !GameNetworkSystem.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			GameNetworkSystem.CreateLobby();
		}
	}

	public void OnActive( Connection channel )
	{
		SpawnPlayer( channel, RedSpawnPoint );
	}

	private void OnDisconnected( Connection channel )
	{

	}

	private void SpawnPlayer( Connection channel, GameObject spawnPoint )
	{
		GameObject player = PlayerPrefab.Clone( spawnPoint.Transform.World, name: channel.DisplayName );
		player.NetworkSpawn( channel );
	}
}
