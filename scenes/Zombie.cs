using Godot;
using System;

public partial class Zombie : CharacterBody2D
{
    [Export] public int Speed = 60;
    private Node2D player;
    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        player = GetTree().GetRoot().GetNode<Node2D>("Main/Player");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null) return;

        Vector2 direction = (player.Position - Position).Normalized();
        Velocity = direction * Speed;

        MoveAndSlide();

        //animace
        if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
        {
            sprite.Play("walk_right");
            sprite.FlipH = direction.X < 0;
        }
        else if (direction.Y > 0)
        {
            sprite.Play("walk_down");
            sprite.FlipH = false;
        }
        else
        {
            sprite.Play("walk_up");
            sprite.FlipH = false;
        }
    }
}
