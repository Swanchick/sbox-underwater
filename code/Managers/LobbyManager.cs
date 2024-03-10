using Sandbox;
using Sandbox.Network;
using System;

public struct Entry
{
	public string Author;
	public string Message;

	public Entry (string author, string message)
	{
		Author = author;
		Message = message;
	}
}

public sealed class LobbyManager : Component, IAutoAssignTeam
{
	[Sync] public Team CurrentTeam { get; set; }

	[Property] public CameraComponent LobbyCamera;
	[Property] public GameObject Menu;

	[Property] public Lobby lobby;

	private GameManager gameManager;

	protected override void OnStart()
	{
		LobbyCamera.Enabled = !IsProxy;

		if ( IsProxy )
		{
			Menu.Destroy();

			return;
		}
	}

	public void SetManager( GameManager manager )
	{
		gameManager = manager;
	}

	public bool CanJoinTeam( Team team )
	{
		NetList<Guid> redPlayers = gameManager.playerRedTeam;
		NetList<Guid> bluePlayers = gameManager.playerBlueTeam;

		Dictionary<Team, NetList<Guid>> teams = new();
		teams.Add( Team.Red, redPlayers );
		teams.Add( Team.Blue, bluePlayers );

		return teams[CurrentTeam].Count > teams[team].Count && CurrentTeam != team;
	}

	public void ChangeTeam(Team team)
	{
		if ( !CanJoinTeam( team ) )
			return;

		gameManager.ChangeTeam( GameObject.Id, CurrentTeam, team );
		CurrentTeam = team;
	}

	public void OnTeamChanged( NetList<Guid> redPlayers, NetList<Guid> bluePlayers )
	{
		lobby.UpdatePlayerList(redPlayers, bluePlayers);
	}

	public void SendAll(string message)
	{
		gameManager.SendAllFuckingMessage(message);
	}
}
