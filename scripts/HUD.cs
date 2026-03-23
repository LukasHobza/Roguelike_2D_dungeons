using Godot;
using System;

public partial class HUD : CanvasLayer
{
    // deklarace popisku pro ruzne statistiky
    private Label LabelHP;
    private Label LabelStamina;
    private Label LabelGold;
    private Label LabelDamage;
    private Label LabelDefence;
    private Label LabelSet;

    // Called when the node enters the scene tree for the first time.
    // inicializace pri nacteni do sceny
    public override void _Ready()
    {
        // najde a ulozi uzel pro zivoty
        LabelHP = GetNode<Label>("VBoxContainer/LabelHP");
        LabelGold = GetNode<Label>("VBoxContainer/LabelGold");
        LabelDamage = GetNode<Label>("VBoxContainer/LabelDamage");
        LabelDefence = GetNode<Label>("VBoxContainer/LabelDefence");
        LabelSet = GetNode<Label>("VBoxContainer/LabelSet");
    }

    // aktualizace textu zivotu
    public void SetHP(int currentHP, int maxHP)
    {
        LabelHP.Text = "Životy: " + currentHP.ToString() + " / " + maxHP.ToString();
    }

    public void SetGold(int gold)
    {
        LabelGold.Text = "Zlato: " + gold.ToString();
    }

    public void SetDamage(int damage)
    {
        LabelDamage.Text = "Poškození: " + damage.ToString();
    }

    public void SetDefence(int defence)
    {
        LabelDefence.Text = "Brnění: " + defence.ToString();
    }

    public void SetSet(bool isActive)
    {
        if (isActive) LabelSet.Text = "Set bonus: 2*";
        else LabelSet.Text = "Set bonus: 1*";        
    }

    // obsluha tlacitka pro navrat
    private void _on_button_pressed()
    {
        // prepne scenu na hlavni menu
        GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
    }
}
