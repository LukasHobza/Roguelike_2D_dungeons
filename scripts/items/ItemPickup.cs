using Godot;

public partial class ItemPickup : Area2D
{
    [Export] public Item ItemData;

    private Sprite2D sprite;

    public override void _Ready()
    {
        // prida objekt do skupiny pro identifikaci
        AddToGroup("ground_item");

        // najde uzel sprite v detech
        sprite = GetNode<Sprite2D>("Sprite2D");

        if (ItemData != null && ItemData.Icon != null)
        {
            // nastavi sprite podle ikony predmetu
            sprite.Texture = ItemData.Icon;
        }

        // pripoji signal pro detekci kolize
        BodyEntered += OnBodyEntered;

        GD.Print("ItemPickup ready");
    }

    private void OnBodyEntered(Node body)
    {
        // kontrola zda do objektu vesel hrac
        if (body is Player player)
        {
            if (ItemData == null)
            {
                GD.PrintErr("ItemData is NULL!");
                return;
            }

            if (player.Inventory.AddItem(ItemData))
            {
                GD.Print("ITEM ADDED:", ItemData.ItemName);
                QueueFree();
            }
        }
    }
}
