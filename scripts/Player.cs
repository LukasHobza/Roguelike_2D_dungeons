using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public int Speed = 150;
	private AnimatedSprite2D sprite;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		//pohyb
		if (Input.IsActionPressed("move_right"))
			velocity.X += 1;
		if (Input.IsActionPressed("move_left"))
			velocity.X -= 1;
		if (Input.IsActionPressed("move_down"))
			velocity.Y += 1;
		if (Input.IsActionPressed("move_up"))
			velocity.Y -= 1;
		
		//vertikalni pohyb nebude rychlejsi
		Velocity = velocity.Normalized() * Speed;
		MoveAndSlide();
		
		string currentAnim = sprite.Animation.ToString();
		if (velocity.Length() == 0)
		{
			//idle animace
			if (currentAnim.Contains("walk"))
			{
				if (currentAnim.Contains("up"))
					sprite.Play("idle_up");
				else if (currentAnim.Contains("down"))
					sprite.Play("idle_down");
				else
					sprite.Play("idle_right");
			}
		}
		else
		{
			//walk animace
			if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
			{
				sprite.Play("walk_right");
				sprite.FlipH = velocity.X < 0;
			}
			else if (velocity.Y < 0)
			{
				sprite.Play("walk_up");
				sprite.FlipH = false;
			}
			else
			{
				sprite.Play("walk_down");
				sprite.FlipH = false;
			}
		}
	}
}
