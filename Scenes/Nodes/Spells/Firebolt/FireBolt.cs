using Godot;
using System;

public partial class Firebolt : Spell
{
    [Export] public int Damage = 40;
    [Export] public float Speed = 100f;
    public override void Cast(SpellParams p)
    {
        
        if (!IsReady) return;

        // Spawn Fire Bolt effect
        var fireBoltEffect = (Node2D)SpellEffectScene.Instantiate();
        fireBoltEffect.GlobalPosition = ((CharacterBody2D)GetParent()).GlobalPosition;
        GetTree().Root.AddChild(fireBoltEffect);

        // Set the direction of the Fire Bolt
        var fireBoltScript = fireBoltEffect as FireboltEffect;
        if (fireBoltScript != null)
        {
            fireBoltScript.Direction = (p.Position - ((CharacterBody2D)GetParent()).GlobalPosition).Normalized();
            fireBoltScript.Damage = Damage;
            fireBoltScript.Speed = Speed;
            fireBoltScript.MaxDistance = this.SpellRange;
        }

        StartCooldown();
    }
}
