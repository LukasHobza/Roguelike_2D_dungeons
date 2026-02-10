using Godot;

public partial class EndLevelSummary : CanvasLayer
{
    [Signal]
    public delegate void NextLevelRequestedEventHandler();

    public override void _Ready()
    {
        var stats = GetNode<RunStats>("/root/RunStats");
        var player = GetTree().GetFirstNodeInGroup("player") as Player;

        GetNode<Label>("Panel/VBoxContainer/LabelTime").Text =
            $"Čas: {stats.LevelTime:0.0} s";

        GetNode<Label>("Panel/VBoxContainer/LabelKills").Text =
            $"Zabito nepřátel: {stats.EnemiesKilled}";

        GetNode<Label>("Panel/VBoxContainer/LabelGold").Text =
            $"Získané zlato: {stats.GoldEarned(player.Gold)}";

        GetNode<Button>("Panel/VBoxContainer/ButtonNext").Pressed += OnNextPressed;
    }

    private void OnNextPressed()
    {
        EmitSignal(SignalName.NextLevelRequested);
        QueueFree();
    }
}
