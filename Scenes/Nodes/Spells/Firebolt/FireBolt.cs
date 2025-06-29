using Godot;
using System;

public partial class Firebolt : Spell
{
    [Export] public int Damage = 40;
    [Export] public float Speed = 500f;
    public override void Cast(SpellParams p)
    {
        Vector2 direction = (p.Position - ((CharacterBody2D)GetParent()).GlobalPosition).Normalized();
        if (!IsReady) return;

        // Spawn Fire Bolt effect
        var fireBoltEffect = (Node2D)SpellEffectScene.Instantiate();
        fireBoltEffect.GlobalPosition = p.Player.GlobalPosition + direction* 150; // Offset to spawn in front of the player
        GetTree().Root.AddChild(fireBoltEffect);

        // Set the direction of the Fire Bolt
        var fireBoltScript = fireBoltEffect as FireboltEffect;
        if (fireBoltScript != null)
        {
            fireBoltScript.Direction = direction;
            fireBoltScript.Damage = Damage;
            fireBoltScript.Speed = Speed;
            fireBoltScript.MaxDistance = this.SpellRange;
            fireBoltScript.Caster = p.Player;
        }

        StartCooldown();
    }
}
