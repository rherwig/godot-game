using Godot;

public class HUD : CanvasLayer
{
	private Label _heartsLabel = null;
	
	private GlobalSignals _globalSignal;

	public override void _Ready()
	{
		_heartsLabel = GetNode<Label>("Panel/MarginContainer/HBoxContainer/HeartsLabel");
		_globalSignal = GetNode<GlobalSignals>(GlobalSignals.NodePath);

		_heartsLabel.Text = "0";

		_globalSignal.Connect(nameof(GlobalSignals.HeartCollected), this, nameof(_collectHeart));
	}

	private void _collectHeart(Player player)
	{
		_heartsLabel.Text = player.GetHealth().ToString();
	}
}
