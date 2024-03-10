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

	[Sync] public NetList<Guid> allPlayers { get; set; } = new();
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

		allPlayers.Add( lobbyClient.Id );
		SetGameManager( lobbyClient.Id );
		AutoAssignTeam( lobbyClient.Id );

	}

	[Broadcast]
	private void SetGameManager(Guid lobbyId)
	{
		GameObject lobby = Scene.Directory.FindByGuid( lobbyId );
		LobbyManager lobbyManager = lobby.Components.Get<LobbyManager>();

		lobbyManager.SetManager( this );
	}

	[Broadcast]
	private void AutoAssignTeam( Guid lobbyId )
	{
		GameObject lobby = Scene.Directory.FindByGuid( lobbyId );

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

		UpdatePlayerTeams( lobbyId );
	}

	[Broadcast]
	public void ChangeTeam(Guid lobbyId, Team oldTeam, Team newTeam)
	{
		if ( !Networking.IsHost )
			return;

		Dictionary<Team, NetList<Guid>> teams = new();
		teams.Add( Team.Red, playerRedTeam );
		teams.Add( Team.Blue, playerBlueTeam );

		teams[oldTeam].Remove( lobbyId );
		teams[newTeam].Add( lobbyId );

		playerRedTeam = teams[Team.Red];
		playerBlueTeam = teams[Team.Blue];

		GameObject lobby = Scene.Directory.FindByGuid( lobbyId );
		SendAllFuckingMessage( "System", $"{lobby.Network.OwnerConnection.DisplayName} has changed team to {newTeam}." );

		UpdatePlayerTeams( lobbyId );
	}

	[Broadcast]
	private void UpdatePlayerTeams(Guid lobbyId)
	{
		GameObject lobby = Scene.Directory.FindByGuid( lobbyId );
		IAutoAssignTeam team = lobby.Components.Get<IAutoAssignTeam>();

		if ( team is null )
			return;

		team.OnTeamChanged( playerRedTeam, playerBlueTeam );
	}

	[Broadcast]
	public void SendAllFuckingMessage(string author, string text)
	{
		foreach (Guid lobbyId in allPlayers)
		{
			GameObject lobby = Scene.Directory.FindByGuid( lobbyId );
			LobbyManager lobbyManager = lobby.Components.Get<LobbyManager>();

			lobbyManager.lobby.AddText( author, text );
		}
	}

	private void OnDisconnected( Connection channel )
	{
		
	}
}
