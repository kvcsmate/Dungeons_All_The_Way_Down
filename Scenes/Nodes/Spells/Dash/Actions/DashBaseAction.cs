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
    private Vector2 _movement;
    private float _previousDashDistance = 0f;
    private Character _caster;

    private SpellAttributes _spellAttributes;

    public override void Execute(SpellAttributes attributes)
    {
        _spellAttributes = attributes;

        if (!_spellAttributes.IsReady)
            return;
        _caster = attributes.Caster;
        _caster.IsDisplaced = true;
        _isDashing = true;

        _caster.StopMovement();
        SetTravelPoint(attributes.Position);

        _movement = _dashDirection * DashSpeed;
        _previousDashDistance = 0f;
    }

    private void SetTravelPoint(Vector2 position)
    {
        _dashDirection = (position - _caster.Position).Normalized();
        _dashDistance = SpellRange;

        var spaceState = GetWorld2D().DirectSpaceState;
        _dashTarget = _caster.Position + _dashDirection * _dashDistance;

        var result = spaceState.IntersectRay(new PhysicsRayQueryParameters2D
        {
            From = _caster.Position,
            To = _dashTarget,
            Exclude = new Godot.Collections.Array<Godot.Rid> { _caster.GetRid() },
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

        float remainingDistance = _caster.Position.DistanceTo(_dashTarget);

        if (!CheckDistance(remainingDistance))
        {
            _caster.Position = _dashTarget;
        }

        if (remainingDistance <= 100)
        {
            _isDashing = false;
            _caster.Velocity = Vector2.Zero;
            _caster.MoveAndSlide();
            _caster.IsDisplaced = false;
        }
        else
        {
            if (Engine.GetPhysicsFrames() % 3 == 0 && _spellAttributes.SpellEffectScene != null)
            {
                var spellEffect = (Node2D)_spellAttributes.SpellEffectScene.Instantiate();
                GetTree().Root.AddChild(spellEffect);
                spellEffect.GlobalPosition = _caster.Position;
            }
            _caster.Velocity = _movement * 100;
            _caster.MoveAndSlide();
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
}
