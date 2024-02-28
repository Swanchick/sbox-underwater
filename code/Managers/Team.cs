using Sandbox;
using System;
using System.Threading.Channels;

public enum Team
{
	None,
	Red,
	Blue
}

public struct PlayerTeam
{
	public Connection PlayerChannel { get; private set; }
	public string DisplayName { get; private set; }
	public Guid PlayerId { get; private set; }
	public Team CurrentTeam { get; private set; }

	public PlayerTeam(Connection playerChannel, Team currentTeam )
	{
		DisplayName = playerChannel.DisplayName;
		PlayerId = playerChannel.Id;
		CurrentTeam = currentTeam;
	}
}
