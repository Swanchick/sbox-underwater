using Sandbox;
using Sandbox.Network;
using System;

public sealed class LobbyManager : Component, IAutoAssignTeam
{
	[Sync] public Team CurrentTeam { get; set; }

	public Action<NetList<Guid>, NetList<Guid>> OnPlayerListUpdate;

	[Property] public CameraComponent LobbyCamera;
	[Property] public GameObject Menu;

	[Property] public Lobby lobby;

	protected override void OnStart()
	{
		LobbyCamera.Enabled = !IsProxy;

		if ( IsProxy )
		{
			Menu.Destroy();

			return;
		}
	}

	public void OnTeamChanged( NetList<Guid> redPlayers, NetList<Guid> bluePlayers )
	{
		lobby.UpdatePlayerList(redPlayers, bluePlayers);
	}
}
