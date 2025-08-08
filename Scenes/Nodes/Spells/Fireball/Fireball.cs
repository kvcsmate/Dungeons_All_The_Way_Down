using Godot;
using System;

public partial class Fireball : Spell
{
    [Export] public int Damage = 50;
    // Called when the node enters the scene tree for the first time.
    public override void Cast(SpellParams p)
    {
        if (!p.IsReady) return;

        // Spawn fireball effect
        var spellEffect = (Node2D)p.SpellEffectScene.Instantiate();
        GetTree().Root.AddChild(spellEffect);
        spellEffect.GlobalPosition = p.Position;

        StartCooldown();
    }

}
