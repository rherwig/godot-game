using Godot;

public class Bat : KinematicBody2D
{
	[Export] private float _speed = 50;

	[Export] private int _direction = -1;

	private AnimatedSprite _sprite = null;
	private CollisionShape2D _collisionShape = null;
	private RayCast2D _floorChecker = null;
	private Area2D _sidesChecker = null;
	private AnimationPlayer _animationPlayer = null;
	private Vector2 _velocity = Vector2.Zero;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_floorChecker = GetNode<RayCast2D>("FloorChecker");
		_sidesChecker = GetNode<Area2D>("SidesChecker");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_updateOrientation();
	}

	public override void _PhysicsProcess(float delta)
	{
		if (IsOnWall() || !_floorChecker.IsColliding() && IsOnFloor())
		{
			_changeDirection();
		}

		_velocity.y += 20;
		_velocity.x = _speed * _direction;

		_velocity = MoveAndSlide(_velocity, Vector2.Up);
	}

	private void _changeDirection()
	{
		_direction *= -1;
		_updateOrientation();
	}

	private void _updateOrientation()
	{
		_sprite.FlipH = _direction > 0;
		_floorChecker.Position = new Vector2(
			(_collisionShape.Shape as RectangleShape2D).Extents.x * _direction,
			_floorChecker.Position.y
		);
	}

	private async void _onTopCheckerBodyEntered(Player player)
	{
		_speed = 0;
		_sprite.Stop();
		_animationPlayer.Play("death");

		SetCollisionLayerBit(4, false);
		SetCollisionMaskBit(0, false);
		_sidesChecker.SetCollisionLayerBit(4, false);
		_sidesChecker.SetCollisionMaskBit(0, false);

		await ToSignal(_animationPlayer, "animation_finished");

		QueueFree();
	}

	private void _onSidesCheckerBodyEntered(Player body)
	{
		body.HandleDamageTaken(1, true);
	}
}
