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
        GD.Print("BODY ENTERED: ", body.Name);

        if (body is Player player)
        {
            GD.Print("PLAYER DETECTED");

            if (player.Inventory.AddItem(ItemData))
            {
                GD.Print("ITEM ADDED");
                QueueFree();
            }
        }
    }
}
