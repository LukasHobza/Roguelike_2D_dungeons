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
        // vyuziva stejnou logiku jako zakladni enemy
        base.UpdateAnimation(direction); // použije základní
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        // pokusi se dropnout loot
        TryDropItem();
        // provede zbytek smrti z enemy
        base.Die();
    }

    private void TryDropItem()
    {
        // pokud nema co dropnout konci
        if (PossibleDrops.Count == 0) return;

        // nahodna kontrola podle sance na drop
        if (GD.Randf() > DropChance)
            return;

        // vyber nahodneho predmetu ze seznamu
        int index = GD.RandRange(0, PossibleDrops.Count - 1);
        Item droppedItem = PossibleDrops[index];

        // nacteni sceny pro predmet na zemi
        var pickupScene = GD.Load<PackedScene>("res://scenes/item_pickup.tscn");
        // vytvoreni instance predmetu
        var pickup = pickupScene.Instantiate<ItemPickup>();

        // prirazeni dat vybraneho predmetu
        pickup.ItemData = droppedItem;
        // nastaveni pozice tam kde zombik umrel
        pickup.GlobalPosition = GlobalPosition;

        // bezpecne pridani do sceny mimo fyzikalni proces
        GetTree().CurrentScene.CallDeferred("add_child", pickup);
    }
}