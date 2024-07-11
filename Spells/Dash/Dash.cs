using Godot;
using System;

public partial class Dash : Spell
{
    [Export] public float DashDistance = 200f;
    [Export] public float DashSpeed = 1000f;
    private Vector2 _dashDirection;
    private float _dashDistance;
    private bool _isDashing = false;

    [Signal]
    public delegate void DashFinishedEventHandler();
    // Called when the node enters the scene tree for the first time.
    public override void Cast(Vector2 position)
    {
        _dashDirection = (position - ((CharacterBody2D)GetParent()).GlobalPosition).Normalized();
        _isDashing = true;

        _dashDistance = DashDistance;
        StartCooldown();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDashing)
        {
            CharacterBody2D parent = (CharacterBody2D)GetParent();
            Vector2 movement = _dashDirection * DashSpeed * (float)delta;
            float remainingDistance = _dashDistance - parent.GlobalPosition.DistanceTo(parent.GlobalPosition + movement);

            GD.Print("parent global pos:" + parent.GlobalPosition);
            GD.Print("movement:" + movement);
            if (remainingDistance <= 0)
            {
                _isDashing = false;
                
                parent.Velocity = _dashDirection * remainingDistance;
                parent.MoveAndSlide(); // Finish the dash movement 
            }
            else
            {
                parent.Velocity = movement;
                parent.MoveAndSlide();
                _dashDistance -= movement.Length();
                EmitSignal(SignalName.DashFinished);
            }
        }
    }

}
