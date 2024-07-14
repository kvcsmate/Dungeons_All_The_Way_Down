using Godot;
using System;

public partial class FireBolt : Spell
{
    [Export] public int Damage = 40;
    [Export] public float Speed = 100f;
    [Export] public float MaxDistance = 500f;

    public override void Cast(Vector2 position)
    {
        if (!IsReady) return;

        // Spawn Fire Bolt effect
        var fireBoltEffect = (Node2D)SpellEffectScene.Instantiate();
        fireBoltEffect.GlobalPosition = ((CharacterBody2D)GetParent()).GlobalPosition;
        GetTree().Root.AddChild(fireBoltEffect);

        // Set the direction of the Fire Bolt
        var fireBoltScript = fireBoltEffect as FireBoltEffect;
        if (fireBoltScript != null)
        {
            fireBoltScript.Direction = (position - ((CharacterBody2D)GetParent()).GlobalPosition).Normalized();
            fireBoltScript.Damage = Damage;
            fireBoltScript.Speed = Speed;
            fireBoltScript.MaxDistance = MaxDistance;
        }

        StartCooldown();
    }
}
