using Sandbox;

public class SUtils
{
	public static int GetButton(string keyName )
	{
		return Input.Down( keyName ) ? 1 : 0;
	}
}
