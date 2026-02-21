using Godot;
using System;

public partial class Shop : Node
{
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
        GetNode<Button>("VBoxContainer/Buy_HP").Pressed += BuyHP;
        GetNode<Button>("VBoxContainer/Buy_Damage").Pressed += BuyDamage;
        GetNode<Button>("VBoxContainer/Buy_Defence").Pressed += BuyDefence;
        GetNode<Button>("VBoxContainer/Back").Pressed += Back;

        goldLabel = GetNode<Label>("VBoxContainer/Gold_Label");
        hpLabel = GetNode<Label>("VBoxContainer/Hp_Level_Label");
        dmgLabel = GetNode<Label>("VBoxContainer/Damage_Level_Label");
        defLabel = GetNode<Label>("VBoxContainer/Defence_Level_Label");

        db = GetNode<GameDatabase>("/root/GameDatabase");
        UpdateUI();
    }

    public void BuyHP()
    {
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
        int gold = db.LoadGold();

        if (gold < price)
        {
            GD.Print("Not enough gold");
            return;
        }

        db.SaveGold(gold - price);
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
        GetTree().ChangeSceneToFile("res://scenes/Main_menu.tscn");
    }
}
