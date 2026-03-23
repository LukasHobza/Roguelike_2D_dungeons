using Godot;

public partial class EndLevelSummary : CanvasLayer
{
    // signal pro vyzadani dalsiho levelu
    [Signal]
    public delegate void NextLevelRequestedEventHandler();

    public override void _Ready()
    {
        // nacteni globalniho uzlu se statistikami
        var stats = GetNode<RunStats>("/root/RunStats");
        // najde hrace pro pristup k jeho penezum
        var player = GetTree().GetFirstNodeInGroup("player") as Player;

        GetNode<Label>("Panel/VBoxContainer/LabelTime").Text =
            $"Čas: {stats.LevelTime:0.0} s";

        GetNode<Label>("Panel/VBoxContainer/LabelKills").Text =
            $"Zabito nepřátel: {stats.EnemiesKilled}";

        GetNode<Label>("Panel/VBoxContainer/LabelGold").Text =
            $"Získané zlato: {stats.GoldEarned(player.Gold)}";

        // pripoji stisknuti tlacitka na funkci
        GetNode<Button>("Panel/VBoxContainer/ButtonNext").Pressed += OnNextPressed;
    }

    private void OnNextPressed()
    {
        // vysle signal pro nacteni dalsi urovne
        EmitSignal(SignalName.NextLevelRequested);
        // odstrani tento panel ze sceny
        QueueFree();
    }
}
