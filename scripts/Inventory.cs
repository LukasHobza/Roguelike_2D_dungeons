using Godot;
using System;

public partial class Inventory : Control
{
    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            Visible = !Visible;
            GetTree().Paused = Visible;
        }
    }
}