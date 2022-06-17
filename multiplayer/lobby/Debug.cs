using Godot;
using System.Collections.Generic;
using System.Linq;

public class Debug : Control
{
	private GameState _gameState;
	private VBoxContainer _playersContainer;

	private bool _isServer;

	public override void _Ready()
	{
		_gameState = GetNode<GameState>(GameState.NodePath);
		_playersContainer = GetNode<VBoxContainer>("PlayersContainer");
		_isServer = OS.GetCmdlineArgs().FirstOrDefault() != "join";

		_gameState.Connect(nameof(GameState.PlayerListChanged), this, nameof(_updatePlayers));

		if (_isServer)
		{
			_gameState.HostGame("Server Player");
		}
		else
		{
			_gameState.JoinGame("127.0.0.1", "Client Player");
		}
	}

	private void _updatePlayers(Dictionary<int, string> networkPlayers)
	{
		foreach (Node child in _playersContainer.GetChildren())
		{
			_playersContainer.RemoveChild(child);
		}

		_playersContainer.AddChild(_createPlayerButton(_gameState.LocalPlayerName, true));

		foreach (var playerName in networkPlayers.Values)
		{
			_playersContainer.AddChild(_createPlayerButton(playerName));
		}
	}

	private Button _createPlayerButton(string playerName, bool isLocalPlayer = false)
	{
		var button = new Button();
		button.Text = playerName;

		if (isLocalPlayer)
		{
			button.Text = $"{button.Text} (you)";
		}

		return button;
	}

	private void _onStartButtonPressed()
	{
		_gameState.StartGame();
		Hide();
	}
}
