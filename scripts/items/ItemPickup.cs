using Godot;

public partial class ItemPickup : Area2D
{
    [Export] public Item ItemData;

    private Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");

        if (ItemData != null && ItemData.Icon != null)
        {
            sprite.Texture = ItemData.Icon;
        }

        BodyEntered += OnBodyEntered;

        GD.Print("ItemPickup ready");
    }

    private void OnBodyEntered(Node body)
    {
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
