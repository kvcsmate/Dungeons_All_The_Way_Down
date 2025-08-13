using Godot;
using System;

public abstract partial class SpellAction : Node2D
{

    [Export]
    public string Id { get; set; }

    [Export]
    public new string Name { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public bool Enabled { get; set; } = false;

    [Export]
    public bool OverrideBaseAction = false;

    public Player Player;

    public abstract void Execute(SpellAttributes spellAttributes);
}
