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

    //veci pro opakovany utok
    [Export] public float AttackCooldown = 1.0f; // jak casto utoci v s
    private float attackTimer = 0.0f;
    private bool isPlayerInRange = false;
    private Player playerToDamage;

    [Signal]
    public delegate void EnemyDiedEventHandler(Enemy enemy);

    public override void _Ready()
    {
        AddToGroup("enemy");

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

        //veci pro opakovany utok
        if (isPlayerInRange && playerToDamage != null)
        {
            attackTimer += (float)delta;
            if (attackTimer >= AttackCooldown)
            {
                playerToDamage.TakeDamage(Damage);
                GD.Print($"{GetType().Name} bit the player again for {Damage}!");
                attackTimer = 0.0f; // reset casovace
            }
        }
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

    protected virtual void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Player p)
        {
            isPlayerInRange = true;
            playerToDamage = p;

            // prvni utok hned pri doteku
            p.TakeDamage(Damage);
            attackTimer = 0.0f;
            GD.Print($"{GetType().Name} dealt initial {Damage} damage!");
        }
    }

    protected virtual void _on_attack_area_body_exited(Node2D body)
    {
        if (body is Player)
        {
            isPlayerInRange = false;
            playerToDamage = null;
            attackTimer = 0.0f;
        }
    }


    /*
    // zásah hráče
    protected virtual void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Player player)
        {
            player.TakeDamage(Damage);
            GD.Print($"{GetType().Name} dealt {Damage} damage!");//GetType().Name nazev tridy aktualniho objektu
        }
    }
    */

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
        RunStats stats = GetNode<RunStats>("/root/RunStats");
        stats.RegisterKill();

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
        {
            player.AddGold(1);
        }

        EmitSignal(SignalName.EnemyDied, this);


        GD.Print($"{GetType().Name} died!");
        QueueFree();
    }
}
