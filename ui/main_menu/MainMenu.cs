using Godot;

public class MainMenu : Control
{
	private VBoxContainer _buttonContainer;
	private Button _startButton;
	
	public override void _Ready()
	{
		_buttonContainer = GetNode<VBoxContainer>("ButtonContainer");
		_startButton = _buttonContainer.GetNode<Button>("StartButton");
		
		_startButton.GrabFocus();
	}

	private void _onStartButtonPressed()
	{
		GetTree().ChangeScene("res://levels/demo.tscn");
	}
	
	private void _onOptionsButtonPressed()
	{
		// Replace with function body.
	}


	private void _onExitButtonPressed()
	{
		GetTree().Quit();
	}
}
