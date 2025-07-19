using Godot;
using System;

public abstract partial class SpellAction : Node
{

    [Export]
    public string Id { get; set; }

    [Export]
    public new string Name { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public bool Enabled { get; set; } = false;

    public virtual void Execute()
    {

    }
}
