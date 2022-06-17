using Godot;

public class Player : KinematicBody2D
{
	[Export] public float MotionSpeed = 600;
	[Export] public float Gravity = 4500;
	[Export] public float JumpStrength = 1500;

	private Vector2 _velocity = Vector2.Zero;

	private AnimatedSprite _sprite;
	private Camera2D _camera;
	private AudioStreamPlayer _soundJump;
	private Vector2 _startScale = Vector2.Zero;

	[Puppet] private Vector2 _puppetVelocity = Vector2.Zero;

	private int _health = 0;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_soundJump = GetNode<AudioStreamPlayer>("SoundJump");
		_camera = GetNode<Camera2D>("Camera2D");
		_startScale = _sprite.Scale;

		if (IsNetworkMaster())
		{
			_camera.MakeCurrent();
		}
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
		var isJumping = Input.IsActionJustPressed("player_jump") && IsOnFloor();
		var isFalling = _velocity.y > 0 && !IsOnFloor();
		var isJumpCancelled = Input.IsActionJustReleased("player_jump");
		var isRunning = IsOnFloor() && !Mathf.IsZeroApprox(_velocity.x);
		var isIdle = IsOnFloor() && Mathf.IsZeroApprox(_velocity.x);

		if (IsNetworkMaster())
		{
			var motionRight = Input.GetActionStrength("player_move_right");
			var motionLeft = Input.GetActionStrength("player_move_left");
			var motionX = motionRight - motionLeft;
			_velocity.x = motionX * MotionSpeed;
			_velocity.y += Gravity * delta;

			if (isJumping)
			{
				_velocity.y = -JumpStrength;
			}
			else if (isJumpCancelled)
			{
				_velocity.y = 0;
			}

			Rset(nameof(_puppetVelocity), _velocity);
		}
		else
		{
			_velocity = _puppetVelocity;
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
