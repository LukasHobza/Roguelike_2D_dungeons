using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private Label LabelHP;
    private Label LabelStamina;
    private Label LabelGold;
    private Label LabelDamage;
    private Label LabelDefence;
    private Label LabelSet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LabelHP = GetNode<Label>("VBoxContainer/LabelHP");
        LabelStamina = GetNode<Label>("VBoxContainer/LabelStamina");
        LabelGold = GetNode<Label>("VBoxContainer/LabelGold");
        LabelDamage = GetNode<Label>("VBoxContainer/LabelDamage");
        LabelDefence = GetNode<Label>("VBoxContainer/LabelDefence");
        LabelSet = GetNode<Label>("VBoxContainer/LabelSet");
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
}
