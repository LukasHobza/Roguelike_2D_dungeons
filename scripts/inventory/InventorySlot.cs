using Godot;

public partial class InventorySlot : Panel
{
    public Item Item;
    private TextureRect icon;

    public override void _Ready()
    {
        icon = GetNode<TextureRect>("Icon");
        UpdateSlot();
    }

    public void SetItem(Item item)
    {
        Item = item;
        UpdateSlot();
    }

    public void Clear()
    {
        Item = null;
        UpdateSlot();
    }

    private void UpdateSlot()
    {
        if (Item != null)
        {
            icon.Texture = Item.Icon;
            icon.Visible = true;
        }
        else
        {
            icon.Texture = null;
            icon.Visible = false;
        }
    }

    public bool IsEmpty()
    {
        return Item == null;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            UseItem();
        }
    }

    private void UseItem()
    {
        if (Item == null) return;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player == null) return;

        if (Item is Potion potion)
        {
            player.Heal(potion.HealAmount);
            Clear(); // item se odebere
        }
        else if (Item is Weapon weapon)
        {
            player.EquipWeapon(weapon);
            // item zustane v invu
        }
    }
}
