using Godot;
using System;

public partial class Zombie : Enemy
{
    [Export] public Godot.Collections.Array<Item> PossibleDrops;
    [Export] public float DropChance = 0.5f; // 50 %

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
        TryDropItem();
        base.Die();
    }

    private void TryDropItem()
    {
        if (PossibleDrops.Count == 0) return;

        if (GD.Randf() > DropChance)
            return;

        int index = GD.RandRange(0, PossibleDrops.Count - 1);
        Item droppedItem = PossibleDrops[index];

        var pickupScene = GD.Load<PackedScene>("res://scenes/item_pickup.tscn");
        var pickup = pickupScene.Instantiate<ItemPickup>();

        pickup.ItemData = droppedItem;
        pickup.GlobalPosition = GlobalPosition;

        GetTree().CurrentScene.CallDeferred("add_child", pickup);
    }
}