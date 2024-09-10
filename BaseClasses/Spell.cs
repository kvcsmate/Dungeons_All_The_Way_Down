using Godot;
using System;

public abstract partial class Spell : Node2D
{
    [Export] public int ManaCost = 10;
    [Export] public float Cooldown = 0.5f;
    [Export] public float SpellRange;
    public bool IsReady = true;
    [Export] public PackedScene SpellEffectScene;
    Player _player;

    public class SpellParams
    {
        public Vector2 Position;
    }
    public abstract void Cast(SpellParams p);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Initialize the spell (e.g., load resources, set up effects)
    }

    protected async void StartCooldown()
    {
        IsReady = false;
        await ToSignal(GetTree().CreateTimer(Cooldown), "timeout");
        IsReady = true;
    }
}
