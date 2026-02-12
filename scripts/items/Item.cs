using Godot;
using System;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public string ItemName = "";
    [Export] public Texture2D Icon;

    [Export] public string SetId = "";
}
