using Sandbox;

public interface IInteractive
{
	bool IsInteractive { get; }

	void OnInteract(Player player);
}
