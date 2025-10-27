using Godot;
using System;

public partial class Zombie : CharacterBody2D
{
    //pohyb
    [Export] public int Speed = 60;
    private Node2D player;
    private AnimatedSprite2D sprite;

    //utoceni/zivoty
    [Export] public int MaxHP = 20;
    public int CurrentHP;

    [Export] public int Damage = 10;

    public override void _Ready()
    {
        player = GetTree().GetRoot().GetNode<Node2D>("Main/Player");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        //nastaveni zivotu
        CurrentHP = MaxHP;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null) return;

        var direction = player.GlobalPosition - GlobalPosition;

        //aby nesel az primo k hraci
        if (direction.Length() > 10)
        {
            Velocity = direction.Normalized() * Speed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

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

    private void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Player player)
        {
            player.TakeDamage(Damage);
            GD.Print("damage: " + Damage);
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            GD.Print("Enemy died!");
            QueueFree(); //smazani zombika
        }
    }
}
