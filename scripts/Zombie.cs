using Godot;
using System;

public partial class Zombie : Enemy
{
    public override void _Ready()
    {
        base._Ready(); // zavolá základní inicializaci z Enemy
        GD.Print("Zombie spawned!");
    }

    protected override void UpdateAnimation(Vector2 direction)
    {
        base.UpdateAnimation(direction); // použije základní
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        base.Die();
    }
}