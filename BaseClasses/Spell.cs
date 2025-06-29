using Godot;
using System;

public abstract partial class Spell : Node2D
{
    [Export] public int ManaCost = 10;
    [Export] public float Cooldown = 0.5f;
    [Export] public float SpellRange;
    public bool IsReady = true;
    public float CooldownRemaining { get; private set; }
    [Export] public PackedScene SpellEffectScene;


    public class SpellParams
    {
        public Vector2 Position;
        public Player Player;
    }
    public abstract void Cast(SpellParams p);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Initialize the spell (e.g., load resources, set up effects)
    }

    protected void StartCooldown()
    {
        IsReady = false;
        CooldownRemaining = Cooldown;
    }

    public override void _Process(double delta)
    {
        if (!IsReady)
        {
            CooldownRemaining -= (float)delta;
            if (CooldownRemaining <= 0)
            {
                CooldownRemaining = 0;
                IsReady = true;
            }
        }
    }
    
}
