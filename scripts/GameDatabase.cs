using Godot;
using System;

public partial class GameDatabase : Node
{
    private string savePath;
    private int _gold;
    private int _hpLevel;
    private int _damageLevel;
    private int _defenceLevel;

    public override void _Ready()
    {
        savePath = "user://save.cfg";
        Load();
    }

    private void Load()
    {
        var cfg = new ConfigFile();
        var err = cfg.Load(savePath);
        if (err == Error.Ok)
        {
            _gold = (int)cfg.GetValue("player", "gold", 0);
            _hpLevel = (int)cfg.GetValue("upgrades", "hp", 0);
            _damageLevel = (int)cfg.GetValue("upgrades", "damage", 0);
            _defenceLevel = (int)cfg.GetValue("upgrades", "defence", 0);
        }
        else
        {
            _gold = 0;
            _hpLevel = 0;
            _damageLevel = 0;
            _defenceLevel = 0;
        }
    }

    private void Save()
    {
        var cfg = new ConfigFile();

        cfg.SetValue("player", "gold", _gold);

        cfg.SetValue("upgrades", "hp", _hpLevel);
        cfg.SetValue("upgrades", "damage", _damageLevel);
        cfg.SetValue("upgrades", "defence", _defenceLevel);

        cfg.Save(savePath);
    }


    //veřejné metody pro Player
    public int LoadGold()
    {
        return _gold;
    }

    public void SaveGold(int gold)
    {
        _gold = gold;
        Save();
    }

    //shopik
    public int GetHpLevel() => _hpLevel;
    public int GetDamageLevel() => _damageLevel;
    public int GetDefenceLevel() => _defenceLevel;

    public void UpgradeHP()
    {
        _hpLevel++;
        Save();
    }

    public void UpgradeDamage()
    {
        _damageLevel++;
        Save();
    }

    public void UpgradeDefence()
    {
        _defenceLevel++;
        Save();
    }
}