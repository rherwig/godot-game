using Godot;

public class Player : KinematicBody2D
{
	private float _speed = 600;
	private float _jumpStrength = 1500;
	private float _gravity = 4500;
	private Vector2 _velocity = Vector2.Zero;

	private AnimatedSprite _sprite = null;
	private AudioStreamPlayer _soundJump;
	private Vector2 _startScale = Vector2.Zero;

	private int _health = 0;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_soundJump = GetNode<AudioStreamPlayer>("SoundJump");
		_startScale = _sprite.Scale;
	}

	public void CollectHeart()
	{
		_health++;
	}

	public int GetHealth()
	{
		return _health;
	}

	public override void _PhysicsProcess(float delta)
	{
		var horizontalDirection =
			Input.GetActionStrength("player_move_right") - Input.GetActionStrength("player_move_left");

		_velocity.y += _gravity * delta;
		_velocity.x = horizontalDirection * _speed;

		var isJumping = Input.IsActionJustPressed("player_jump") && IsOnFloor();
		var isFalling = _velocity.y > 0 && !IsOnFloor();
		var isJumpCancelled = Input.IsActionJustReleased("player_jump");
		var isRunning = IsOnFloor() && !Mathf.IsZeroApprox(_velocity.x);
		var isIdle = IsOnFloor() && Mathf.IsZeroApprox(_velocity.x);

		if (isJumping)
		{
			_velocity.y = -_jumpStrength;
		}
		else if (isJumpCancelled)
		{
			_velocity.y = 0;
		}

		if (isJumping)
		{
			_soundJump.Play();
			_sprite.Play("jump");
		}
		else if (isFalling)
		{
			_sprite.Play("fall");
		}
		else if (isRunning)
		{
			_sprite.Play("run");
		}
		else if (isIdle)
		{
			_sprite.Play("idle");
		}

		_velocity = MoveAndSlide(_velocity, Vector2.Up);
		
		if (!Mathf.IsZeroApprox(_velocity.x))
		{
			_sprite.Scale = new Vector2(Mathf.Sign(_velocity.x) * _startScale.x, _sprite.Scale.y);
		}
	}
}
