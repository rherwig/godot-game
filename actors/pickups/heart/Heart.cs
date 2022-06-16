using Godot;

public class Heart : Area2D
{
	private AnimationPlayer _animationPlayer = null;

	private GlobalSignals _globalSignal;
	
	public override void _Ready()
	{
		_globalSignal = GetNode<GlobalSignals>(GlobalSignals.NodePath);
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	private async void _onHeartBodyEntered(Player body)
	{
		body.CollectHeart();

		_globalSignal.EmitSignal(nameof(GlobalSignals.HeartCollected), body);
		_animationPlayer.Play("bounce");
		
		await ToSignal(_animationPlayer, "animation_finished");
		
		QueueFree();
	}
}
