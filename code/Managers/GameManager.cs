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
	[Property] public GameObject BlueSpawnPoint { get; set; }
	
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

	private void OnActive( Connection channel )
	{
		if ( IsRoundStarted )
		{
			SpawnPlayer( channel, RedSpawnPoint );
			
			return;
		}
		
		PlayerTeam playerTeam = new PlayerTeam( channel, Team.None );
		PlayersTeam.Add( playerTeam );
	}

	private void OnDisconnected( Connection channel )
	{
		RemovePlayerTeam( channel.Id );
	}

	private void RemovePlayerTeam(Guid id)
	{
		foreach (PlayerTeam playerTeam in PlayersTeam )
		{
			if (playerTeam.PlayerId == id )
			{
				PlayersTeam.Remove( playerTeam );

				break;
			}
		}
	}
	
	[Broadcast]
	public void ChangeTeam(Guid id, Team team)
	{
		if ( !Networking.IsHost )
			return;

		Connection channel = Networking.FindConnection( id );
		PlayerTeam playerTeam = new PlayerTeam( channel, team );

		RemovePlayerTeam( id );

		PlayersTeam.Add( playerTeam );
	}

	private void SpawnPlayer( Connection channel, GameObject spawnPoint )
	{
		GameObject player = PlayerPrefab.Clone( spawnPoint.Transform.World, name: channel.DisplayName );
		player.NetworkSpawn( channel );
	}

	public void StartRound()
	{
		if ( IsRoundStarted )
			return;

		IsRoundStarted = true;

		if ( PlayerPrefab is null )
			return;
		
		IReadOnlyList<Connection> connections = Networking.Connections;
		
		foreach (PlayerTeam playerTeam in PlayersTeam)
		{
			Connection channel = Networking.FindConnection( playerTeam.PlayerId );

			if (playerTeam.CurrentTeam == Team.Red )
			{
				SpawnPlayer( channel, RedSpawnPoint );
			}
			else if (playerTeam.CurrentTeam == Team.Blue )
			{
				SpawnPlayer( channel, BlueSpawnPoint );
			}
		}

		DeleteLobby();
	}

	[Broadcast]
	private void DeleteLobby()
	{
		LobbyObject.Destroy();
	}
}
