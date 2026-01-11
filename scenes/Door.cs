using Godot;

public partial class Door : Area2D
{
    [Signal]
    public delegate void DoorEnteredEventHandler();

    private bool triggered = false;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (triggered) return;
        if (!area.IsInGroup("player_interact")) return;

        triggered = true;
        CallDeferred(nameof(EmitDoor));
    }

    private void EmitDoor()
    {
        EmitSignal(SignalName.DoorEntered);
    }
}
