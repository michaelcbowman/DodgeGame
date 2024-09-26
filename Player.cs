using Godot;
using System;
public enum State
{
	Cracked,
	Healthy
}

public partial class Player : Area2D
{
	[Signal]
	public delegate void HitEventHandler();
	[Signal]
	public delegate void GetHarderEventHandler();

	[Export]
	public int Speed { get; set; } = 400; // How fast the player will move (pixels/sec)

	public Vector2 ScreenSize; // Size of the game window

	public State PlayerState { get; set; } = State.Cracked;

	private State CurrentState { get; set; }

    public override void _Ready()
    {
        ScreenSize = GetViewportRect().Size;
		Hide();
    }

    public override void _Process(double delta)
    {
        Vector2 velocity = Vector2.Zero; // The player's movement vector

		if (Input.IsActionPressed("move_right")) velocity.X += 1;
		if (Input.IsActionPressed("move_left")) velocity.X -= 1;
		if (Input.IsActionPressed("move_down")) velocity.Y += 1;
		if (Input.IsActionPressed("move_up")) velocity.Y -= 1;

		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();
		}
		else animatedSprite2D.Stop();

		Position += velocity * (float)delta;
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);

		UpdateAnimation(velocity, animatedSprite2D);
    }

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("milk"))
		{
			body.Hide();
			EmitSignal(SignalName.GetHarder);
		}
		if (body.IsInGroup("mobs"))	EmitSignal(SignalName.Hit);
	}

	private void UpdateAnimation(Vector2 velocity, AnimatedSprite2D animatedSprite2D)
	{
		string walkAnimation;
		switch (PlayerState)
		{
			case State.Healthy:
				walkAnimation = "healthy_up";
				break;
			default:
				walkAnimation = "cracked_up";
				break;
		}
		
		if (!string.IsNullOrEmpty(walkAnimation))
		{
			if (velocity.X != 0) animatedSprite2D.Animation = walkAnimation;
			else if (velocity.Y != 0) animatedSprite2D.Animation = walkAnimation;
		}
	}

	private void OnHit()
	{
		switch (PlayerState)
		{
			case State.Healthy:
				PlayerState = State.Cracked;
				break;

			case State.Cracked:
				Hide(); // Player disappears after being hit
				EmitSignal(SignalName.Hit);
				GetNode<CollisionShape2D>("CollisionShape2D")
					.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
				break;
		}
		
	}
}
