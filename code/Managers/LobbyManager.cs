using Sandbox;
using Sandbox.Network;
using System;

public sealed class LobbyManager : Component, IAutoAssignTeam
{
	[Sync] public Team CurrentTeam { get; set; }

	[Property] public CameraComponent LobbyCamera;
	[Property] public GameObject Menu;

	[Property] public Lobby lobby;

	public GameManager GameManager { get; set; }

	protected override void OnStart()
	{
		LobbyCamera.Enabled = !IsProxy;

		if ( IsProxy )
		{
			Menu.Destroy();

			return;
		}
	}

	public bool CanJoinTeam( Team team )
	{
		NetList<Guid> redPlayers = GameManager.playerRedTeam;
		NetList<Guid> bluePlayers = GameManager.playerBlueTeam;

		Dictionary<Team, NetList<Guid>> teams = new();
		teams.Add( CurrentTeam, redPlayers );
		teams.Add( team, bluePlayers );

		return teams[CurrentTeam].Count >= teams[team].Count;
	}

	public void ChangeTeam(Team team)
	{

	}

	public void OnTeamChanged( NetList<Guid> redPlayers, NetList<Guid> bluePlayers )
	{
		lobby.UpdatePlayerList(redPlayers, bluePlayers);
	}
}
