using Sandbox;
using System;

public abstract class BaseInteractive : Component, IInteractive
{
	public virtual bool IsInteractive { get; protected set; } = true;

	public virtual void OnInteract( Guid userId ) { }

	protected GameObject FindObject( Guid objectId )
	{
		return Scene.Directory.FindByGuid( objectId );
	}
}
