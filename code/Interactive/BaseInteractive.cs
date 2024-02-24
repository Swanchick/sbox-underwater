using Sandbox;
using System;

public abstract class BaseInteractive : Component, IInteractive
{
	public virtual bool IsInteractive { get; private set; } = true;

	[Broadcast]
	public virtual void OnInteract( Guid userId ) { }
}
