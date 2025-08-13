using Godot;
using System;

public partial class NPC_Fireball : Area2D
{
    [Export] public float Speed = 420f;
    [Export] public float Lifetime = 3f;
    [Export] public int Damage = 10;

    private Vector2 _direction = Vector2.Zero;

    // Enemy calls this right after spawn
    public void SetDirection(Vector2 dir)
    {
        _direction = dir.Normalized();
        Rotation = _direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Move
        GlobalPosition += _direction * Speed * (float)delta;

        // Lifetime
        Lifetime -= (float)delta;
        if (Lifetime <= 0f)
            QueueFree();

        // Simple hit detection (no signal hookup needed)
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body.IsInGroup("Player"))
            {
                // If your player has a TakeDamage(int) method, uncomment:
                // body.Call("TakeDamage", Damage);
                QueueFree();
                break;
            }
        }
    }
}
