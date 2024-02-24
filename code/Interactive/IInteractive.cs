using Sandbox;
using System;

public interface IInteractive
{
	bool IsInteractive { get; }

	void OnInteract(Guid userId);
}
