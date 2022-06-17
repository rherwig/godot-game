using System;
using Godot;

public class HUD : CanvasLayer
{
	private GlobalSignals _globalSignal;

	public override void _Ready()
	{
		_globalSignal = GetNode<GlobalSignals>(GlobalSignals.NodePath);

		_globalSignal.Connect(nameof(GlobalSignals.HeartCollected), this, nameof(_collectHeart));
	}

	public void UpdateHeartHud(int health, int diff)
	{
		if (diff == 0)
		{
			return;
		}

		// player got healed
		if (diff > 0)
		{
			for (var i = 0; i < health; i++)
			{
				var heartIcon = GetNode<TextureRect>("Panel/MarginContainer/HBoxContainer/HeartTexture" +
													 (health - diff - i).ToString());
				heartIcon.Visible = true;
			}

			return;
		}

		// player got damaged
		for (var i = health + diff; i < health; i++)
		{
			var heartIcon = GetNode<TextureRect>("Panel/MarginContainer/HBoxContainer/HeartTexture" +
												 (i + 1).ToString());
			heartIcon.Visible = false;
		}
	}

	private void _collectHeart(Player player)
	{
		UpdateHeartHud(player.GetHealth(), 1);
	}
}
