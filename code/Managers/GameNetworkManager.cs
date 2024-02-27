using Sandbox;
using Sandbox.Network;
using System;
using System.Threading.Tasks;

[Title( "Game Network Manager" )]
[Category( "Managers" )]
[Icon( "electrical_services" )]
public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; }
	[Property] public bool testSpawn { get; set; } = false;
	[Property] public GameObject LobbyObject { get; set; }


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
		PlayerTeam playerTeam = new PlayerTeam( channel, Team.None );
		
		PlayersTeam.Add( playerTeam );

		Log.Info( PlayersTeam.Count );
	}

	public void StartRound()
	{
		if ( IsRoundStarted )
			return;

		IsRoundStarted = true;

		if ( PlayerPrefab is null )
			return;

		GameObject spawnPoint = SpawnPoints[0];
		IReadOnlyList<Connection> connections = Networking.Connections;
		
		foreach (Connection channel in connections )
		{
			GameObject player = PlayerPrefab.Clone( spawnPoint.Transform.World, name: channel.DisplayName );
			player.NetworkSpawn( channel );
		}

		DeleteLobby();
	}

	[Broadcast]
	private void DeleteLobby()
	{
		LobbyObject.Enabled = false;
	}
}
