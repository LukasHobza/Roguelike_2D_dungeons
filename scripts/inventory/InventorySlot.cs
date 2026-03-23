using Godot;
using static System.Formats.Asn1.AsnWriter;

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

    // metoda pro prekresleni slotu
    private void UpdateSlot()
    {
        // kontrola zda item i ikona existuji
        if (Item != null && Item.Icon != null)
        {
            // nastavi texturu z dat itemu
            icon.Texture = Item.Icon;
            // zobrazi uzel ikony
            icon.Visible = true;
        }
        // pokud je slot prazdny
        else
        {
            // odstrani texturu z uzlu
            icon.Texture = null;
            // skryje uzel ikony
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
        // overeni zda jde o kliknuti tlacitkem mysi
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            // kontrola zda bylo stisknuto leve tlacitko
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                UseItem();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                DropItem();
            }
        }
    }

    private void DropItem()
    {
        if (Item == null) return;

        // najde hrace podle skupiny ve scene
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player == null) return;

        // kontrola zda jde o zbran
        if (Item is Weapon weapon)
        {
            // pokud ma hrac tuhle zbran v ruce
            if (player.EquippedWeapon == weapon)
            {
                player.UnequipWeapon();
            }
        }
        else if (Item is Armor armor)
        {
            if (player.EquippedArmor == armor)
            {
                player.UnequipArmor();
            }
        }
        SpawnPickup(Item);
        Clear();
    }

    private void SpawnPickup(Item item)
    {
        // nacteni sceny pro vyhozeny predmet
        var pickupScene = GD.Load<PackedScene>("res://scenes/item_pickup.tscn");
        // vytvoreni instance teto sceny
        var pickup = pickupScene.Instantiate<ItemPickup>();

        // prirazeni dat itemu do noveho objektu
        pickup.ItemData = item;

        // najde hrace pro urceni pozice vyhozeni
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        // nastavi pozici pod hrace s malym posunem
        pickup.GlobalPosition = player.GlobalPosition + new Vector2(0, 32);

        // prida objekt do aktualni spustene sceny
        GetTree().CurrentScene.AddChild(pickup);
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
        else if (Item is Armor armor)
        {
            player.EquipArmor(armor);
            // armor zůstává v inventáři
        }
    }
}
