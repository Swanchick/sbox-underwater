using Sandbox;
using Sandbox.Network;
using Sandbox.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;

[Title( "Game Manager" )]
[Category( "Managers" )]
[Icon( "electrical_services" )]
public sealed class GameManager : Component, Component.INetworkListener
{
	[Property] public GameObject LobbyClientPrefab { get; set; }
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject RedSpawnPoint { get; set; }

	[Sync] public bool IsRoundStarted { get; set; } = false;

	[Sync] public NetList<Guid> playerRedTeam { get; set; } = new();
	[Sync] public NetList<Guid> playerBlueTeam { get; set; } = new();

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
		if ( IsRoundStarted )
		{

			return;
		}

		GameObject lobbyClient = LobbyClientPrefab.Clone();
		lobbyClient.Name = $"Lobby: {channel.DisplayName}";
		lobbyClient.NetworkSpawn( channel );

		LobbyManager lobbyManager = lobbyClient.Components.Get<LobbyManager>();
		lobbyManager.GameManager = this;

		AutoAssignTeam( lobbyClient );
	}

	private void AutoAssignTeam( GameObject lobby )
	{
		IAutoAssignTeam team = lobby.Components.Get<IAutoAssignTeam>();

		if ( team is null )
			return;

		if ( playerRedTeam.Count == playerBlueTeam.Count || playerRedTeam.Count < playerBlueTeam.Count )
		{
			playerRedTeam.Add( lobby.Id );
			team.CurrentTeam = Team.Red;
		}
		else
		{
			playerBlueTeam.Add( lobby.Id );
			team.CurrentTeam = Team.Blue;
		}

		UpdatePlayerTeams( lobby.Id );
	}

	public void ChangeTeam(GameObject lobby, Team team)
	{

	}

	[Broadcast]
	private void UpdatePlayerTeams(Guid lobbyId)
	{
		GameObject lobby = Scene.Directory.FindByGuid( lobbyId );

		IAutoAssignTeam team = lobby.Components.Get<IAutoAssignTeam>();
		Log.Info( team );

		if ( team is null )
			return;

		team.OnTeamChanged( playerRedTeam, playerBlueTeam );
	}

	private void OnDisconnected( Connection channel )
	{
		
	}
}
