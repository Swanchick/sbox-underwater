using Sandbox;
using System;

public enum Team
{
	None,
	Red,
	Blue
}


interface IAutoAssignTeam
{
	public Team CurrentTeam { get; set; }

	public void OnTeamChanged( NetList<Guid> redPlayers, NetList<Guid> bluePlayer );
}
