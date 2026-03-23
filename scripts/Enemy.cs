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

    // signal pri smrti
    [Signal]
    public delegate void EnemyDiedEventHandler(Enemy enemy);

    public override void _Ready()
    {
        // pridani do skupiny nepratel
        AddToGroup("enemy");

        // najde hrace v hlavni scene
        player = GetTree().GetRoot().GetNode<Node2D>("Main/Player");
        // najde uzel pro animace
        sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        CurrentHP = MaxHP;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null) return;

        // smer k hraci
        var direction = player.GlobalPosition - GlobalPosition;

        // zabrani pronikani do hrace
        if (direction.Length() > 10)
        {
            // pohyb smerem k hraci
            Velocity = direction.Normalized() * Speed;
        }
        else
        {
            // zastavi se
            Velocity = Vector2.Zero;
        }

        MoveAndSlide();

        UpdateAnimation(direction);

        //veci pro opakovany utok
        if (isPlayerInRange && playerToDamage != null)
        {
            // pricitani casu
            attackTimer += (float)delta;
            // pokud vyprsel cooldown
            if (attackTimer >= AttackCooldown)
            {
                // udeli poskozeni hraci
                playerToDamage.TakeDamage(Damage);
                GD.Print($"{GetType().Name} bit the player again for {Damage}!");
                // vynulovani casovace
                attackTimer = 0.0f; // reset casovace
            }
        }
    }

    // univerzalni animace chůze
    protected virtual void UpdateAnimation(Vector2 direction) // neni pristupna zvenci protected
    {
        if (sprite == null) return;

        // pokud je pohyb vic do stran nez nahoru/dolu
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
        // je to hrac
        if (body is Player p)
        {
            // hrac je v dosahu
            isPlayerInRange = true;
            // ulozeni odkazu na hrace
            playerToDamage = p;

            // prvni utok hned pri doteku
            p.TakeDamage(Damage);
            // reset casovace pro dalsi utok
            attackTimer = 0.0f;
            GD.Print($"{GetType().Name} dealt initial {Damage} damage!");
        }
    }

    protected virtual void _on_attack_area_body_exited(Node2D body)
    {
        // odchazi hrac
        if (body is Player)
        {
            isPlayerInRange = false;
            playerToDamage = null;
            attackTimer = 0.0f;
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
        // nacteni globalnich statistik
        RunStats stats = GetNode<RunStats>("/root/RunStats");
        // zapocteni zabiti
        stats.RegisterKill();

        // najde hrace
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
        {
            // prida hraci zlato
            player.AddGold(1);
        }

        // odesle signal o smrti
        EmitSignal(SignalName.EnemyDied, this);


        GD.Print($"{GetType().Name} died!");
        // odstrani nepritele ze scen
        QueueFree();
    }
}
