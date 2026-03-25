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

    private bool isInvulnerable = true;

    //damage
    [Export] public int BaseDamage = 15;
    public int Damage;
    public Weapon EquippedWeapon;

    private bool setBonusActive = false;
    private Sprite2D weapon;
    private Area2D attackArea;
    private bool isAttacking = false;
    private Vector2 facingDirection = Vector2.Right;

    //zvuk
    private AudioStreamPlayer attackSound;
    private AudioStreamPlayer hurtSound;


    public void MakeInvulnerable(float duration)
    {
        isInvulnerable = true;
        // po uplynuti casu nesmrtelnost vypne
        GetTree().CreateTimer(duration).Timeout += () => isInvulnerable = false;
    }
    //
    public override void _Ready()
	{
        // nacteni databaze a zlata
        var db = GetNode<GameDatabase>("/root/GameDatabase");
        Gold = db.LoadGold();
        GD.Print("Loaded gold: ", Gold);

        // aplikace vylepseni z obchodu
        int hpLvl = db.GetHpLevel();
        int dmgLvl = db.GetDamageLevel();
        int defLvl = db.GetDefenceLevel();

        MaxHP += hpLvl * 10;
        BaseDamage += dmgLvl * 2;
        BaseDefence += defLvl * 1;

        // nalezeni inventare
        Inventory = GetTree().GetFirstNodeInGroup("inventory") as Inventory;

        // prirazeni uzlu
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        weapon = GetNode<Sprite2D>("Weapon");
        attackArea = GetNode<Area2D>("AttackArea");

        weapon.Visible = false;
        attackArea.Monitoring = false;//Objekt aktivně sleduje kolize / vstupy

        // vychozi animace
        sprite.Play("idle_right");

        // nastaveni zakladnich statistik
        Defence = BaseDefence;
        Damage = BaseDamage;

        //zvuk
        attackSound = GetNode<AudioStreamPlayer>("AttackSound");
        hurtSound = GetNode<AudioStreamPlayer>("HurtSound");

        //nastaveni zivotu
        CurrentHP = MaxHP;
        UpdateHUD();

        // docasna nesmrtelnost po startu
        GetTree().CreateTimer(1.0f).Timeout += () => isInvulnerable = false;
    }

    // pridani zlata a ulozeni
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
        // ziskani smeru z klavesnice
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_up",
            "move_down"
        );

        // reseni pohybu pri utoku
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

        // logika animaci pohybu a stani
        if (!isAttacking)
        {
            if (input == Vector2.Zero)
            {
                // idle animace podle posledniho smeru
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
                // walk animace podle vstupu
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

        // kontrola stisku tlacitka pro utok
        if (Input.IsActionJustPressed("attack") && !isAttacking)
            Attack();
    }

    // detekce zasahu nepritele
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

        //prehrani zvuku
        if (weapon.Texture != null) attackSound.Play();

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

        // delka trvani utoku
        await ToSignal(GetTree().CreateTimer(0.40f), "timeout");

        // reset stavu po utoku
        weapon.Visible = false;
        attackArea.Monitoring = false;
        isAttacking = false;

        sprite.Play(previousAnimation);//vrati animaci pred utokem
    }


    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        // vypocet damage po zapocteni obrany
        int damage = Mathf.Max(amount - Defence, 0);
        CurrentHP -= damage;

        hurtSound.Play();

        if (CurrentHP < 0) CurrentHP = 0;
        UpdateHUD();

        // smrt hrace
        if (CurrentHP == 0)
        {
            GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
            GD.Print("Player died!");
        }
    }

    // aktualizace grafiky hud
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

    public void EquipWeapon(Weapon weaponData)
    {
        EquippedWeapon = weaponData;

        // nastaveni obrazku pro zbran hrace
        if (weaponData != null && weaponData.Icon != null)
        {
            weapon.Texture = weaponData.Icon;
        }

        RecalculateStats();
    }

    public void UnequipWeapon()
    {
        EquippedWeapon = null;

        weapon.Texture = null;

        RecalculateStats();
    }

    public void EquipArmor(Armor armorData)
    {
        EquippedArmor = armorData;
        RecalculateStats();
    }

    public void UnequipArmor()
    {
        EquippedArmor = null;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        // zaklad
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
