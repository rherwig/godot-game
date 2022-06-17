using Godot;

public class Player : KinematicBody2D
{
	[Export] public float MotionSpeed = 600;
	[Export] public float Gravity = 4500;
	[Export] public float JumpStrength = 1500;

	public int MAXIMUM_HEALTH = 2;

	private Vector2 _velocity = Vector2.Zero;

	private GameState _gameState;
	private AnimatedSprite _sprite;
	private Camera2D _camera;
	private AudioStreamPlayer _soundJump;
	private Vector2 _startScale = Vector2.Zero;

	private int _health = 1;
	private bool _invulnerable = false;
	private float _invulnerabilityTimer;

	[Puppet] private Vector2 _puppetVelocity = Vector2.Zero;

	public override void _Ready()
	{
		_gameState = GetNode<GameState>(GameState.NodePath);
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_soundJump = GetNode<AudioStreamPlayer>("SoundJump");
		_camera = GetNode<Camera2D>("Camera2D");
		_startScale = _sprite.Scale;

		if (IsNetworkMaster())
		{
			_camera.MakeCurrent();
		}
	}

	// Handles Health increase on Heart pickup and returns new Health on success and -1 if Health is already full
	public int CollectHeart()
	{
		if (_health >= MAXIMUM_HEALTH) return -1;
		_health++;
		return GetHealth();
	}

	public int GetHealth()
	{
		return _health;
	}

	public void HandleDamageTaken(int damage, bool knockback = false)
	{
		_health -= damage;
		if (_health < 1)
		{
			HandleDeath();
			return;
		}

		var hud = GetTree().Root.GetNode<HUD>("LevelDemo/HUD");
		hud.UpdateHeartHud(GetHealth(), damage * -1);

		EnterGracePeriod();

		if (knockback)
		{
			_velocity.x = _sprite.Scale.x * -1;
		}
	}

	void EnterGracePeriod()
	{
		_invulnerable = true;
		_invulnerabilityTimer = 1.0f;
	}

	public void HandleDeath()
	{
		_gameState.Respawn(GetTree().GetNetworkUniqueId());
	}

	public override void _Process(float delta)
	{
		if (!_invulnerable) return;
		_invulnerabilityTimer -= delta;
		if (!(_invulnerabilityTimer < 0)) return;
		_invulnerable = false;
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
