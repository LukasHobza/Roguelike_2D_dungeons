using Godot;

public partial class InventorySlot : Panel
{
    public Item Item;
    private TextureRect icon;

    public override void _Ready()
    {
        icon = GetNode<TextureRect>("Icon");
        Clear();
    }

    //Nastavení itemu do slotu
    public void SetItem(Item item)
    {
        Item = item;
        UpdateSlot();
    }

    //Vymazání slotu
    public void Clear()
    {
        Item = null;
        UpdateSlot();
    }

    //Aktualizace UI
    private void UpdateSlot()
    {
        if (Item != null && Item.Icon != null)
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

    //Kontrola, jestli je slot prázdný
    public bool IsEmpty()
    {
        return Item == null;
    }

    //Kliknutí myší na slot
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            UseItem();
        }
    }

    //Použití itemu
    private void UseItem()
    {
        if (Item == null) return;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player == null) return;

        if (Item is Potion potion)
        {
            player.Heal(potion.HealAmount);
            Clear(); // potion se spotřebuje
        }
        else if (Item is Weapon weapon)
        {
            player.EquipWeapon(weapon);
            // zbran zůstává v inventáři
        }
    }
}
