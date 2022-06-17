using Godot;

public class DebugSinglePlayer : Node2D
{
    private GameState _gameState;

    public override void _Ready()
    {
        _gameState = GetNode<GameState>(GameState.NodePath);
        _gameState.HostGame("Local Player");
        _gameState.StartGame();
    }
}
