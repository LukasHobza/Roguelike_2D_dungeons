using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//tlacitka
        var ButtonStart = GetNode<Button>("CenterContainer/VBoxContainer/ButtonPlay");
        var ButtonQuit = GetNode<Button>("CenterContainer/VBoxContainer/ButtonQuit");

        //bude volat tuto fci po kliknuti
        ButtonStart.Pressed += OnStartPressed;
        ButtonQuit.Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        GD.Print("play");
        //nacte hlavni scenu hry
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    private void OnQuitPressed()
    {
        GD.Print("quit");
        GetTree().Quit();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
