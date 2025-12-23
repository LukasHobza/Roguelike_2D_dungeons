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
}
