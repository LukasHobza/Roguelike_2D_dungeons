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
        // vytvori instanci konfiguracniho souboru
        var cfg = new ConfigFile();
        // pokusi se nacist soubor z cesty
        var err = cfg.Load(savePath);
        // pokud se soubor uspesne nacetl
        if (err == Error.Ok)
        {
            // nacte zlato nebo nulu pokud neexistuje
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
        // vytvori novy objekt souboru
        var cfg = new ConfigFile();

        // zapise aktualni zlato do sekce player
        cfg.SetValue("player", "gold", _gold);

        cfg.SetValue("upgrades", "hp", _hpLevel);
        cfg.SetValue("upgrades", "damage", _damageLevel);
        cfg.SetValue("upgrades", "defence", _defenceLevel);

        // fyzicky ulozi soubor na disk
        cfg.Save(savePath);
    }

    //veřejné metody pro Player
    // vrati mnozstvi nacteneho zlata
    public int LoadGold()
    {
        return _gold;
    }

    // ulozi nove zlato a zapise na disk
    public void SaveGold(int gold)
    {
        _gold = gold;

        // provede zapis do souboru
        Save();
    }

    //shopik
    // vraci uroven zivotu pro obchod
    public int GetHpLevel() => _hpLevel;
    public int GetDamageLevel() => _damageLevel;
    public int GetDefenceLevel() => _defenceLevel;

    // vylepseni zivotu
    public void UpgradeHP()
    {
        _hpLevel++;
        // ulozi zmenu
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