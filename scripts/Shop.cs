using Godot;
using System;

public partial class Shop : Node
{
    // ceny vylepseni nastavitelne v editoru
    [Export] public int HpPrice = 20;
    [Export] public int DamagePrice = 20;
    [Export] public int DefencePrice = 20;

    private Label goldLabel;
    private Label hpLabel;
    private Label dmgLabel;
    private Label defLabel;

    private GameDatabase db;

    public override void _Ready()
    {
        // pripojeni tlacitek na prislusne funkce
        GetNode<Button>("VBoxContainer/Buy_HP").Pressed += BuyHP;
        GetNode<Button>("VBoxContainer/Buy_Damage").Pressed += BuyDamage;
        GetNode<Button>("VBoxContainer/Buy_Defence").Pressed += BuyDefence;
        GetNode<Button>("VBoxContainer/Back").Pressed += Back;

        // nalezeni vsech textovych poli v ui
        goldLabel = GetNode<Label>("VBoxContainer/Gold_Label");
        hpLabel = GetNode<Label>("VBoxContainer/Hp_Level_Label");
        dmgLabel = GetNode<Label>("VBoxContainer/Damage_Level_Label");
        defLabel = GetNode<Label>("VBoxContainer/Defence_Level_Label");

        // nacteni globalniho uzlu databaze
        db = GetNode<GameDatabase>("/root/GameDatabase");
        // prvni vykresleni textu
        UpdateUI();
    }

    // vola se pri koupi zivota
    public void BuyHP()
    {
        // zavola univerzalni funkci pro nakup
        TryBuy(HpPrice, () => db.UpgradeHP());
    }

    public void BuyDamage()
    {
        TryBuy(DamagePrice, () => db.UpgradeDamage());
    }

    public void BuyDefence()
    {
        TryBuy(DefencePrice, () => db.UpgradeDefence());
    }

    private void TryBuy(int price, Action upgrade)
    {
        // nacte aktualni stav penez
        int gold = db.LoadGold();

        // kontrola zda ma hrac dostatek zlata
        if (gold < price)
        {
            GD.Print("Not enough gold");
            return;
        }

        // odecte cenu a ulozi novy stav penez
        db.SaveGold(gold - price);
        // provede konkretni vylepseni
        upgrade.Invoke();

        UpdateUI();
    }

    private void UpdateUI()
    {
        goldLabel.Text = "Gold: " + db.LoadGold();

        hpLabel.Text = "HP Level: " + db.GetHpLevel() + " | Cost: " + HpPrice;
        dmgLabel.Text = "Damage Level: " + db.GetDamageLevel() + " | Cost: " + DamagePrice;
        defLabel.Text = "Defence Level: " + db.GetDefenceLevel() + " | Cost: " + DefencePrice;
    }


    private void Back()
    {
        // prepne scenu na hlavni menu
        GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
    }
}
