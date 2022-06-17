using Godot;

public class GlobalSignals : Node
{
	public const string NodePath = "/root/GlobalSignals";
	
	[Signal]
	public delegate void HeartCollected(Player player);
}
