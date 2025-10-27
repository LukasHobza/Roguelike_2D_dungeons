using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private Label LabelHP;
    private Label LabelStamina;
    private Label LabelGold;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LabelHP = GetNode<Label>("VBoxContainer/LabelHP");
        LabelStamina = GetNode<Label>("VBoxContainer/LabelStamina");
        LabelGold = GetNode<Label>("VBoxContainer/LabelGold");
    }

    public void SetHP(int currentHP, int maxHP)
    {
        LabelHP.Text = "Životy: " + currentHP.ToString() + " / " + maxHP.ToString();
    }

    public void SetStamina(int currentStamina)
    {
        LabelStamina.Text = "Výdrž: " + currentStamina.ToString();
    }

    public void SetGold(int gold)
    {
        LabelGold.Text = "Zlato: " + gold.ToString();
    }
}
