using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{
    // seznam vsech slotu v inventari
    private List<InventorySlot> slots = new();

    public override void _Ready()
    {
        // na zacatku je inventar skryty
        Visible = false;
        // bezi i pri pauze hry
        ProcessMode = ProcessModeEnum.Always;

        // projde vsechny uzly ve skupine slotu
        foreach (Node node in GetTree().GetNodesInGroup("inventory_slot"))
        {
            GD.Print("Found slot: ", node.Name);

            // pokud je uzel spravneho typu prida ho do seznamu
            if (node is InventorySlot slot)
                slots.Add(slot);
        }

        GD.Print("Total slots: ", slots.Count);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // stisknuti klavesy pro inventar
        if (Input.IsActionJustPressed("inventory"))
        {
            // prepne viditelnost okna
            Visible = !Visible;
            // pauzne hru pokud je inventar otevreny
            GetTree().Paused = Visible;
        }
    }


    public bool AddItem(Item item)
    {
        foreach (var slot in slots)
        {
            // hleda prvni prazdny slot
            if (slot.IsEmpty())
            {
                slot.SetItem(item);
                return true;
            }
        }

        GD.Print("Inventory full!");
        return false;
    }
}