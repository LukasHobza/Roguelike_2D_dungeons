using Godot;

public partial class Settings : Control
{
    private OptionButton resolutionOption;
    private HSlider volumeSlider;

    public override void _Ready()
    {
        resolutionOption = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer/OptionButton");
        volumeSlider = GetNode<HSlider>("Panel/VBoxContainer/HBoxContainer2/HSlider");

        // aktuální hlasitost v decibelech z hlavního busu 0
        float currentDb = AudioServer.GetBusVolumeDb(0);
        // převedeme zpet na lineární hodnotu 0.0 - 1.0 a pak na 0-100 pro slider
        volumeSlider.Value = Mathf.DbToLinear(currentDb) * 100f;

        resolutionOption.Clear();
        resolutionOption.AddItem("800x600");
        resolutionOption.AddItem("1280x720");
        resolutionOption.AddItem("1600x900");
        resolutionOption.AddItem("1920x1080");

        // nastaveni správneho indexu v menu podle aktualni velikosti okna
        Vector2I windowSize = DisplayServer.WindowGetSize();
        string currentResString = $"{windowSize.X}x{windowSize.Y}";

        for (int i = 0; i < resolutionOption.ItemCount; i++)
        {
            if (resolutionOption.GetItemText(i) == currentResString)
            {
                resolutionOption.Selected = i;
                break;
            }
        }

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
