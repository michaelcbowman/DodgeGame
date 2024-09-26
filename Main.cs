using Godot;
using System;
using System.ComponentModel;

public partial class Main : Node
{
	[Export]
	public PackedScene MobScene { get; set; }
	[Export]
	public PackedScene MilkScene { get; set;}

	private int _score;

	private Player _player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();
		GetNode<Timer>("PowerupTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();

		GetNode<HUD>("HUD").ShowGameOver();
		GetNode<AudioStreamPlayer>("Music").Stop();
		GetNode<AudioStreamPlayer>("DeathSound").Play();
	}

	public void NewGame()
	{
		_score = 0;

		_player = GetNode<Player>("Player");
		Marker2D startPosition = GetNode<Marker2D>("StartPosition");
		_player.Start(startPosition.Position);

		GetTree().CallGroup("mobs", Node.MethodName.QueueFree);
		GetTree().CallGroup("milk", Node.MethodName.QueueFree);
		GetNode<Timer>("StartTimer").Start();

		HUD hud = GetNode<HUD>("HUD");
		hud.UpdateScore(_score);
		hud.ShowMessage("Get Ready!");
		GetNode<AudioStreamPlayer>("Music").Play();
	}

	public void OnHit()
	{
		switch (_player.PlayerState)
		{
			case State.Healthy:
				_player.PlayerState = State.Cracked;
				_player.GetNode<CollisionShape2D>("CollisionShape2D")
					.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
				GetNode<Timer>("InvincibilityTimer").Start();
				break;
			case State.Cracked:
				_player.Hide(); // Player disappears after being hit
				GameOver();
				_player.GetNode<CollisionShape2D>("CollisionShape2D")
					.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
				break;
		}
	}

	private void OnInvincibilityTimerTimeout()
	{
		_player.GetNode<CollisionShape2D>("CollisionShape2D")
				.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
	}

	private void OnMobTimerTimeout()
	{
		Mob mob = MobScene.Instantiate<Mob>();
		PathFollow2D mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");

		Spawn(mob, mobSpawnLocation);
	}

	private void OnPowerUpTimerTimeout()
	{
		Milk milk = MilkScene.Instantiate<Milk>();
		PathFollow2D milkSpawnLocation = GetNode<PathFollow2D>("MilkPath/MilkSpawnLocation");
		
		Spawn(milk, milkSpawnLocation);
	}

	private void OnScoreTimerTimeout()
	{
		_score++;

		GetNode<HUD>("HUD").UpdateScore(_score);
	}

	private void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
		GetNode<Timer>("PowerupTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();
	}

	private void Spawn(RigidBody2D sprite, PathFollow2D spawnLocation)
	{
		// Choose a random location on Path2D
		spawnLocation.ProgressRatio = GD.Randf();

		// Set the mob's direction perpendicular to the path direction
		float direction = spawnLocation.Rotation + Mathf.Pi / 2;

		// Set the mob's position to a random location
		sprite.Position = spawnLocation.Position;

		// Add some randomness to the direction
		direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
		sprite.Rotation = direction;

		// Choose the velocity
		Vector2 velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
		sprite.LinearVelocity = velocity.Rotated(direction);

		// Spawn the mob by adding it to the Main scene
		AddChild(sprite);
	}

	public void GotMilk()
	{
		if (_player.PlayerState == State.Cracked) _player.PlayerState = State.Healthy;
	}
}
