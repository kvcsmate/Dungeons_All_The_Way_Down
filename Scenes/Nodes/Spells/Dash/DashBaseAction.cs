using Godot;
using System;

public partial class DashBaseAction : SpellAction
{
    [Export] public float DashSpeed = 100f;
    [Export] public float SpellRange = 200f; // Exposed for tuning in the editor

    private Vector2 _dashDirection;
    private float _dashDistance;
    private Vector2 _dashTarget;
    private bool _isDashing = false;
    private Player _player;
    private Vector2 _movement;
    private float _previousDashDistance = 0f;

    private SpellParams _spellParams;

    public override void Execute(SpellParams p)
    {
        _spellParams = p;

        if (!p.IsReady)
            return;

        _player = p.Player;
        _player.IsDisplaced = true;
        _isDashing = true;

        _player.StopMovement();
        SetTravelPoint(p.Position);

        _movement = _dashDirection * DashSpeed;
        _previousDashDistance = 0f;
    }

    private void SetTravelPoint(Vector2 position)
    {
        _dashDirection = (position - _player.Position).Normalized();
        _dashDistance = SpellRange;

        var spaceState = GetWorld2D().DirectSpaceState;
        _dashTarget = _player.Position + _dashDirection * _dashDistance;

        var result = spaceState.IntersectRay(new PhysicsRayQueryParameters2D
        {
            From = _player.Position,
            To = _dashTarget,
            Exclude = new Godot.Collections.Array<Godot.Rid> { _player.GetRid() },
            CollisionMask = 1
        });

        if (result.Count > 0)
        {
            // Stop before the collider
            _dashTarget = ((Vector2)result["position"]) - _dashDirection * 30;
        }
        // Optionally: _player.CreateIndicator(_dashTarget);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isDashing)
            return;

        float remainingDistance = _player.Position.DistanceTo(_dashTarget);

        if (!CheckDistance(remainingDistance))
        {
            _player.Position = _dashTarget;
        }

        if (remainingDistance <= 100)
        {
            _isDashing = false;
            _player.Velocity = Vector2.Zero;
            _player.MoveAndSlide();
            _player.IsDisplaced = false;
            StartCooldown();
        }
        else
        {
            if (Engine.GetPhysicsFrames() % 3 == 0 && _spellParams.SpellEffectScene != null)
            {
                var spellEffect = (Node2D)_spellParams.SpellEffectScene.Instantiate();
                GetTree().Root.AddChild(spellEffect);
                spellEffect.GlobalPosition = _player.Position;
            }
            _player.Velocity = _movement * 100;
            _player.MoveAndSlide();
        }
    }

    private bool CheckDistance(float distance)
    {
        GD.Print($"Distance: {distance} Previous Distance: {_previousDashDistance}");
        if (_previousDashDistance != 0f && _previousDashDistance < distance)
        {
            GD.Print("Dash distance too far, resetting dash.");
            return false;
        }
        else
        {
            _previousDashDistance = distance;
            return true;
        }
    }
    private void StartCooldown()
    {
        if (_spellParams != null)
        {
            _spellParams.IsReady = false;
            _spellParams.CooldownRemaining = _spellParams.Cooldown;
        }
        
    }

}
