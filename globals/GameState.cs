using Godot;
using System.Collections.Generic;

public class GameState : Node
{
    public const string NodePath = "/root/GameState";

    public string LocalPlayerName { get; set; }

    private const int DefaultPort = 47111;
    private const int MaxPlayers = 2;

    private NetworkedMultiplayerENet _peer;

    private Dictionary<int, string> _networkPlayers = new Dictionary<int, string>();
    private List<int> _networkPlayersReady = new List<int>();

    [Signal]
    public delegate void PlayerListChanged();

    private void _onPlayerConnected(int id)
    {
        RpcId(id, nameof(RegisterPlayer), LocalPlayerName);

        GD.Print($"Player connected: {id} - {LocalPlayerName}");
    }

    private void _onPlayerDisconnected(int id)
    {
        RpcId(id, nameof(UnregisterPlayer), LocalPlayerName);

        GD.Print($"Player disconnected: {id} - {LocalPlayerName}");
    }

    public void HostGame(string localPlayerName)
    {
        LocalPlayerName = localPlayerName;
        _peer = new NetworkedMultiplayerENet();

        _peer.CreateServer(DefaultPort, MaxPlayers);

        GetTree().NetworkPeer = _peer;
    }

    public void JoinGame(string ip, string localPlayerName)
    {
        LocalPlayerName = localPlayerName;
        _peer = new NetworkedMultiplayerENet();

        _peer.CreateClient(ip, DefaultPort);

        GetTree().NetworkPeer = _peer;
    }

    [Remote]
    public void RegisterPlayer(string playerName)
    {
        var id = GetTree().GetRpcSenderId();
        _networkPlayers.Add(id, playerName);

        GD.Print($"Player registered: {id} - {playerName}");

        EmitSignal(nameof(PlayerListChanged), _networkPlayers);
    }

    [Remote]
    public void UnregisterPlayer(int id)
    {
        _networkPlayers.Remove(id);

        GD.Print($"Player registered: {id}");

        EmitSignal(nameof(PlayerListChanged), _networkPlayers);
    }

    [Remote]
    public void PreStartGame(Dictionary<int, int> spawnPoints)
    {
        GetTree().Paused = true;

        var level = GD.Load<PackedScene>("res://levels/demo.tscn").Instance();
        GetTree().Root.CallDeferred("add_child", level);

        var playerScene = GD.Load<PackedScene>("res://actors/player/player.tscn");

        foreach (var spawnPoint in spawnPoints)
        {
            var playerId = spawnPoint.Key;
            var spawnPointIndex = spawnPoint.Value;

            var spawnPosition = level.GetNode<Node2D>($"SpawnPoints/Spawn{spawnPointIndex}").Position;
            var player = playerScene.Instance<Player>();

            player.Name = playerId.ToString();
            player.Position = spawnPosition;
            player.SetNetworkMaster(playerId);

            level.AddChild(player);
        }

        if (!GetTree().IsNetworkServer())
        {
            RpcId(1, nameof(PlayerReady), GetTree().GetNetworkUniqueId());
        }
        else if (_networkPlayers.Count == 0)
        {
            PostStartGame();
        }
    }

    [Remote]
    public void PlayerReady(int id)
    {
        if (!GetTree().IsNetworkServer())
        {
            return;
        }

        if (!_networkPlayersReady.Contains(id))
        {
            _networkPlayersReady.Add(id);
        }

        if (_networkPlayers.Count != _networkPlayersReady.Count)
        {
            return;
        }

        foreach (var networkPlayerId in _networkPlayers.Keys)
        {
            RpcId(networkPlayerId, nameof(PostStartGame));
        }

        PostStartGame();
    }

    [Remote]
    public void PostStartGame()
    {
        GetTree().Paused = false;
    }

    public void StartGame()
    {
        if (!GetTree().IsNetworkServer())
        {
            return;
        }

        var spawnPoints = new Dictionary<int, int>();
        var spawnIndex = 0;

        spawnPoints.Add(GetTree().GetNetworkUniqueId(), spawnIndex);

        foreach (var networkPlayerId in _networkPlayers.Keys)
        {
            spawnIndex++;
            spawnPoints.Add(networkPlayerId, spawnIndex);
        }

        foreach (var networkPlayerId in _networkPlayers.Keys)
        {
            RpcId(networkPlayerId, nameof(PreStartGame), spawnPoints);
        }

        PreStartGame(spawnPoints);
    }

    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, nameof(_onPlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(_onPlayerDisconnected));
    }
}
