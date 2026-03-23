using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // nacteni tlacitka pro start
        var ButtonStart = GetNode<Button>("CenterContainer/VBoxContainer/ButtonPlay");
        var ButtonQuit = GetNode<Button>("CenterContainer/VBoxContainer/ButtonQuit");
        var ButtonSettings = GetNode<Button>("CenterContainer/VBoxContainer/ButtonSettings");
        var ButtonShop = GetNode<Button>("CenterContainer/VBoxContainer/ButtonShop");

        //bude volat tuto fci po kliknuti
        ButtonStart.Pressed += OnStartPressed;
        ButtonQuit.Pressed += OnQuitPressed;
        ButtonSettings.Pressed += OnSettingsPressed;
        ButtonShop.Pressed += OnShopPressed;
    }

    private void OnStartPressed()
    {
        GD.Print("play");
        //nacte hlavni scenu hry
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    private void OnSettingsPressed()
    {
        GD.Print("settings");
        //nacte hlavni scenu hry
        GetTree().ChangeSceneToFile("res://scenes/Settings.tscn");
    }

    private void OnQuitPressed()
    {
        GD.Print("quit");
        // ukonceni aplikace
        GetTree().Quit();
    }

    private void OnShopPressed()
    {
        // prepnuti do sceny obchodu
        GetTree().ChangeSceneToFile("res://scenes/Shop.tscn");
    }
}
