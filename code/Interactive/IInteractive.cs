using Sandbox;
using System;

public interface IInteractive
{
	bool IsInteractive { get; }

	[Broadcast]
	void OnInteract(Guid userId);
}
