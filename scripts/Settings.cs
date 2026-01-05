using Godot;

public partial class Settings : Control
{
    private OptionButton resolutionOption;
    private HSlider volumeSlider;

    public override void _Ready()
    {
        resolutionOption = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer/OptionButton");
        volumeSlider = GetNode<HSlider>("Panel/VBoxContainer/HBoxContainer2/HSlider");

        resolutionOption.Clear();
        resolutionOption.AddItem("800x600");
        resolutionOption.AddItem("1280x720");
        resolutionOption.AddItem("1600x900");
        resolutionOption.AddItem("1920x1080");

        volumeSlider.MinValue = 0;
        volumeSlider.MaxValue = 100;
        volumeSlider.Value = 80;

        volumeSlider.ValueChanged += OnVolumeChanged;
        resolutionOption.ItemSelected += OnResolutionSelected;
    }

    private void OnResolutionSelected(long index)
    {
        string text = resolutionOption.GetItemText((int)index);
        var parts = text.Split("x");

        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);
        GD.Print(parts[0] + " x " + parts[1]);

        DisplayServer.WindowSetSize(new Vector2I(width, height));
    }

    private void OnVolumeChanged(double value)
    {
        float db = Mathf.LinearToDb((float)value / 100f);
        AudioServer.SetBusVolumeDb(0, db);
    }

    public void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
    }
}
