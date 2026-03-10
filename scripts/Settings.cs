using Godot;

public partial class Settings : Control
{
    private OptionButton resolutionOption;
    private HSlider volumeSlider;

    public override void _Ready()
    {
        resolutionOption = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer/OptionButton");
        volumeSlider = GetNode<HSlider>("Panel/VBoxContainer/HBoxContainer2/HSlider");

        // --- Hlasitost ---
        float currentDb = AudioServer.GetBusVolumeDb(0);
        volumeSlider.Value = Mathf.DbToLinear(currentDb) * 100f;

        // --- Rozlišení ---
        resolutionOption.Clear();
        resolutionOption.AddItem("800x600");
        resolutionOption.AddItem("1280x720");
        resolutionOption.AddItem("1600x900");
        resolutionOption.AddItem("1920x1080");

        // Získá aktuální rozlišení
        Vector2I currentRes = GetTree().Root.ContentScaleSize;

        if (currentRes == Vector2I.Zero)
        {
            currentRes = DisplayServer.WindowGetSize();
        }

        string currentResString = $"{currentRes.X}x{currentRes.Y}";
        GD.Print("Hledám rozlišení: " + currentResString);

        bool found = false;
        for (int i = 0; i < resolutionOption.ItemCount; i++)
        {
            if (resolutionOption.GetItemText(i) == currentResString)
            {
                resolutionOption.Selected = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            GD.Print("Aktuální rozlišení není v seznamu OptionButton.");
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
        Vector2I newRes = new Vector2I(width, height);

        // Změní velikost fyzického okna
        DisplayServer.WindowSetSize(newRes);

        GetTree().Root.ContentScaleSize = newRes;

        // Vycentrování okna po změně rozlišení
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        DisplayServer.WindowSetPosition(screenSize / 2 - newRes / 2);

        GD.Print($"Resolution changed to: {width}x{height}");
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
