using Godot;

public partial class Settings : Control
{
    private OptionButton resolutionOption;
    private HSlider volumeSlider;

    public override void _Ready()
    {
        // najde uzel pro vyber rozliseni
        resolutionOption = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer/OptionButton");
        // najde uzel pro posuvnik hlasitosti
        volumeSlider = GetNode<HSlider>("Panel/VBoxContainer/HBoxContainer2/HSlider");

        // ziska aktualni hlasitost hlavni sbernice
        float currentDb = AudioServer.GetBusVolumeDb(0);
        // prevede decibely na hodnotu 0 az 100 pro slider
        volumeSlider.Value = Mathf.DbToLinear(currentDb) * 100f;

        // vymaze stare polozky v seznamu
        resolutionOption.Clear();
        // prida moznosti rozliseni
        resolutionOption.AddItem("800x600");
        resolutionOption.AddItem("1280x720");
        resolutionOption.AddItem("1600x900");
        resolutionOption.AddItem("1920x1080");

        // ziska aktualni velikost vykreslovani
        Vector2I currentRes = GetTree().Root.ContentScaleSize;

        // pokud neni nastaveno meritko ziska velikost okna
        if (currentRes == Vector2I.Zero)
        {
            currentRes = DisplayServer.WindowGetSize();
        }

        // vytvori textovy retezec rozliseni
        string currentResString = $"{currentRes.X}x{currentRes.Y}";
        GD.Print("Hledám rozlišení: " + currentResString);

        // pokus o nalezeni aktualniho rozliseni v seznamu
        bool found = false;
        for (int i = 0; i < resolutionOption.ItemCount; i++)
        {
            if (resolutionOption.GetItemText(i) == currentResString)
            {
                // pokud se text shoduje vybere polozku
                resolutionOption.Selected = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            GD.Print("Aktuální rozlišení není v seznamu OptionButton.");
        }

        // pripojeni zmeny hlasitosti na funkci
        volumeSlider.ValueChanged += OnVolumeChanged;
        // pripojeni zmeny rozliseni na funkci
        resolutionOption.ItemSelected += OnResolutionSelected;
    }

    private void OnResolutionSelected(long index)
    {
        // ziska text vybrane polozky
        string text = resolutionOption.GetItemText((int)index);
        var parts = text.Split("x");

        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);
        // vytvori novy vektor rozliseni
        Vector2I newRes = new Vector2I(width, height);

        // Změní velikost fyzického okna
        DisplayServer.WindowSetSize(newRes);

        // nastavi meritko obsahu sceny
        GetTree().Root.ContentScaleSize = newRes;

        // ziska celkovou velikost obrazovky monitoru
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        // vypocita pozici pro vycentrovani okna
        DisplayServer.WindowSetPosition(screenSize / 2 - newRes / 2);

        GD.Print($"Resolution changed to: {width}x{height}");
    }

    private void OnVolumeChanged(double value)
    {
        // prevede hodnotu 0-100 na decibely
        float db = Mathf.LinearToDb((float)value / 100f);
        // nastavi hlasitost na hlavni sbernici
        AudioServer.SetBusVolumeDb(0, db);
    }

    public void OnBackPressed()
    {
        // prepne scenu zpet do hlavniho menu
        GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
    }
}
