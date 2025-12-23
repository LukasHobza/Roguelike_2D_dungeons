using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class Player : CharacterBody2D
{
	//veci pro pohyb
	[Export] public int Speed = 100;
	private AnimatedSprite2D sprite;
    private string previousAnimation = "walk_right";

    //zivoty
    [Export] public int MaxHP = 100;
    public int CurrentHP;

    //damage
    [Export] public int AttackDamage = 15;
    private Sprite2D weapon;
    private Area2D attackArea;
    private bool isAttacking = false;
    private Vector2 facingDirection = Vector2.Right;

    //
    public override void _Ready()
	{
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        weapon = GetNode<Sprite2D>("Weapon");
        attackArea = GetNode<Area2D>("AttackArea");

        weapon.Visible = false;
        attackArea.Monitoring = false;//Objekt aktivně sleduje kolize / vstupy

        sprite.Play("idle_right");
        //nastaveni zivotu
        CurrentHP = MaxHP;
        UpdateHUD();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (isAttacking) return;

        //veci pro pohyb hrace
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

        //veci pro utoceni hrace
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        
        if (!isAttacking)
        {
            Velocity = input * Speed;
            MoveAndSlide();

            if (input != Vector2.Zero)
                facingDirection = input; // směr, kam se hráč dívá
        }

        if (Input.IsActionJustPressed("attack") && !isAttacking)
            Attack();
    }

    private void _on_attack_area_body_entered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(AttackDamage);
            GD.Print($"Player dealt {AttackDamage} damage to {enemy.GetType().Name}!");
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
            weapon.ZIndex = 0;
            weapon.Offset = new Vector2(20, 2);
            sprite.Play("attack_down");
        }
        else if (facingDirection.X < 0)
        {
            weapon.ZIndex = 0;
            weapon.Offset = new Vector2(26, 0);
            sprite.Play("attack_right");
        }
        else
        {
            weapon.ZIndex = 0;
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
        CurrentHP -= amount;
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
    }
}
