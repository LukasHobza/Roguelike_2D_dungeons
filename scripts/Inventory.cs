using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{
    private List<InventorySlot> slots = new();

    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;

        foreach (Node node in GetTree().GetNodesInGroup("inventory_slot"))
        {
            GD.Print("Found slot: ", node.Name);

            if (node is InventorySlot slot)
                slots.Add(slot);
        }

        GD.Print("Total slots: ", slots.Count);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            Visible = !Visible;
            GetTree().Paused = Visible;
        }
    }


    public bool AddItem(Item item)
    {
        foreach (var slot in slots)
        {
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