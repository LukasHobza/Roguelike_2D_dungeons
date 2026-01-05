using Godot;

public partial class Enemy : CharacterBody2D
{
    // pohyb
    [Export] public int Speed = 60;
    protected Node2D player;
    protected AnimatedSprite2D sprite;

    // zivoty / utok
    [Export] public int MaxHP = 20;
    [Export] public int Damage = 10;
    public int CurrentHP;

    public override void _Ready()
    {
        player = GetTree().GetRoot().GetNode<Node2D>("Main/Player");
        sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        CurrentHP = MaxHP;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null) return;

        var direction = player.GlobalPosition - GlobalPosition;

        // zabrani pronikani do hrace
        if (direction.Length() > 10)
        {
            Velocity = direction.Normalized() * Speed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        MoveAndSlide();

        UpdateAnimation(direction);
    }

    // univerzalni animace chůze
    protected virtual void UpdateAnimation(Vector2 direction) // neni pristupna zvenci protected
    {
        if (sprite == null) return;

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

    // zásah hráče
    protected virtual void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Player player)
        {
            player.TakeDamage(Damage);
            GD.Print($"{GetType().Name} dealt {Damage} damage!");//GetType().Name nazev tridy aktualniho objektu
        }
    }

    // poškození od hráče
    public virtual void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
        {
            player.AddGold(1);
        }


        GD.Print($"{GetType().Name} died!");
        QueueFree();
    }
}
