using Godot;
using System;

public partial class Item : Resource
{
    [Export] public string ItemName = "";
    [Export] public Texture2D Icon;
}
