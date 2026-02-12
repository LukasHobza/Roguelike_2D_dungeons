using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class Player : CharacterBody2D
{
    public int Gold { get; private set; }
    public Inventory Inventory;

    //veci pro pohyb
    [Export] public int Speed = 100;
	private AnimatedSprite2D sprite;
    private string previousAnimation = "walk_right";

    //zivoty
    [Export] public int MaxHP = 100;
    public int CurrentHP;
    public int Defence = 0;
    public int BaseDefence = 0;
    public Armor EquippedArmor;

    //damage
    [Export] public int BaseDamage = 15;
    public int Damage;
    public Weapon EquippedWeapon;

    private bool setBonusActive = false;
    private Sprite2D weapon;
    private Area2D attackArea;
    private bool isAttacking = false;
    private Vector2 facingDirection = Vector2.Right;

    //
    public override void _Ready()
	{
        var db = GetNode<GameDatabase>("/root/GameDatabase");
        Gold = db.LoadGold();
        GD.Print("Loaded gold: ", Gold);

        Inventory = GetTree().GetFirstNodeInGroup("inventory") as Inventory;

        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        weapon = GetNode<Sprite2D>("Weapon");
        attackArea = GetNode<Area2D>("AttackArea");

        weapon.Visible = false;
        attackArea.Monitoring = false;//Objekt aktivně sleduje kolize / vstupy

        sprite.Play("idle_right");

        Defence = BaseDefence;
        Damage = BaseDamage;

        //nastaveni zivotu
        CurrentHP = MaxHP;
        UpdateHUD();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        var db = GetNode<GameDatabase>("/root/GameDatabase");
        db.SaveGold(Gold);
        GD.Print("Gold: ", Gold);

        UpdateHUD();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        // --- INPUT ---
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_up",
            "move_down"
        );

        // --- POHYB ---
        if (isAttacking)
        {
            Velocity = Vector2.Zero;
        }
        else
        {
            Velocity = input.Normalized() * Speed;

            if (input != Vector2.Zero)
                facingDirection = input;
        }

        MoveAndSlide();

        // --- ANIMACE ---
        if (!isAttacking)
        {
            if (input == Vector2.Zero)
            {
                if (Math.Abs(facingDirection.X) > Math.Abs(facingDirection.Y))
                {
                    sprite.Play("idle_right");
                    sprite.FlipH = facingDirection.X < 0;
                }
                else if (facingDirection.Y < 0)
                {
                    sprite.Play("idle_up");
                    sprite.FlipH = false;
                }
                else
                {
                    sprite.Play("idle_down");
                    sprite.FlipH = false;
                }
            }
            else
            {
                if (Math.Abs(input.X) > Math.Abs(input.Y))
                {
                    sprite.Play("walk_right");
                    sprite.FlipH = input.X < 0;
                }
                else if (input.Y < 0)
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

        // --- UTOK ---
        if (Input.IsActionJustPressed("attack") && !isAttacking)
            Attack();
    }


    private void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
            GD.Print($"Player dealt {Damage} damage to {enemy.GetType().Name}!");
        }
    }

    private async void Attack()
    {
        isAttacking = true;
        weapon.Visible = true;
        attackArea.Monitoring = true;

        previousAnimation = sprite.Animation;//ulozi animaci pred utokem

        // natočení meče podle směru
        if (facingDirection.X < 0)
            weapon.RotationDegrees = 180;
        else if (facingDirection.Y < 0)
            weapon.RotationDegrees = -90;
        else if (facingDirection.Y > 0)
            weapon.RotationDegrees = 90;
        else
            weapon.RotationDegrees = 0;

        attackArea.Position = weapon.Position;   // attackArea se přesune tam kde je meč
        attackArea.RotationDegrees = weapon.RotationDegrees;

        // krátká animace útoku podle směru
        if (facingDirection.Y < 0)
        {
            weapon.ZIndex = -1;
            weapon.Offset = new Vector2(20, -2);
            sprite.Play("attack_up");
        }
        else if (facingDirection.Y > 0)
        {
            weapon.ZIndex = 1;
            weapon.Offset = new Vector2(20, 2);
            sprite.Play("attack_down");
        }
        else if (facingDirection.X < 0)
        {
            weapon.ZIndex = 1;
            weapon.Offset = new Vector2(26, 0);
            sprite.Play("attack_right");
        }
        else
        {
            weapon.ZIndex = 1;
            weapon.Offset = new Vector2(26, 0);
            sprite.Play("attack_right");
        }

        await ToSignal(GetTree().CreateTimer(0.40f), "timeout");


        weapon.Visible = false;
        attackArea.Monitoring = false;
        isAttacking = false;

        sprite.Play(previousAnimation);//vrati animaci pred utokem
    }


    public void TakeDamage(int amount)
    {
        int damage = Mathf.Max(amount - Defence, 0);
        CurrentHP -= damage;
        if (CurrentHP < 0) CurrentHP = 0;
        UpdateHUD();

        if (CurrentHP == 0)
        {
            GD.Print("Player died!");
        }
    }

    private void UpdateHUD()
    {
        var hud = GetTree().GetRoot().GetNode<HUD>("Main/UI/HUD");
        hud.SetHP(CurrentHP,MaxHP);
        hud.SetGold(Gold);
        hud.SetDamage(Damage);
        hud.SetDefence(Defence);
        hud.SetSet(setBonusActive);
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);//Mathf.Min() aby hp nebylo vice nez maximum
        UpdateHUD();
    }

    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
        RecalculateStats();
    }

    public void UnequipWeapon()
    {
        EquippedWeapon = null;
        RecalculateStats();
    }

    public void EquipArmor(Armor armor)
    {
        EquippedArmor = armor;
        RecalculateStats();
    }

    public void UnequipArmor()
    {
        EquippedArmor = null;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        // zasklad
        Damage = BaseDamage;
        Defence = BaseDefence;

        if (EquippedWeapon != null)
            Damage += EquippedWeapon.Damage;

        if (EquippedArmor != null)
            Defence += EquippedArmor.Defense;

        setBonusActive = false;

        //SET BONUS
        if (EquippedWeapon != null && EquippedArmor != null)
        {
            if (!string.IsNullOrEmpty(EquippedWeapon.SetId) &&
                EquippedWeapon.SetId == EquippedArmor.SetId)
            {
                Damage *= 2;
                Defence *= 2;
                setBonusActive = true;

                GD.Print("SET BONUS ACTIVE");
            }
        }

        UpdateHUD();
    }

}
