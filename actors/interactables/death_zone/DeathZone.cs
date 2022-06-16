using Godot;

public class DeathZone : Area2D
{
	public override void _Ready()
	{
		
	}
	
	private void _onDeathZoneBodyEntered(object body)
	{
		GetTree().ReloadCurrentScene();
	}
}
