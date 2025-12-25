using Godot;

public partial class ItemPickup : Area2D
{
    [Export] public Item ItemData;

    public override void _Ready()
    {
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
