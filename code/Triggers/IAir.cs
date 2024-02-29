using Sandbox;

public interface IAir
{
	public List<GameObject> AirTrigger { get; set; }

	public void OnAirEnter( GameObject trigger );
	public void OnAirEnterWithParent( GameObject trigger, GameObject parent );
	public void OnAirLeave( GameObject trigger );
}
