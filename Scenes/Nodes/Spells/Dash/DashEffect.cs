using Godot;
using System;

public partial class DashEffect : SpellEffect
{
    [Export] public float DashSpeed = 100f;

    private Vector2 _dashDirection;
    private Vector2 _dashTarget;
    private bool _isDashing;
    private Vector2 _movement;
    private float _previousDashDistance;
    private Character _caster;

    public override void Activate(SpellAttributes spellAttributes)
    {
        if (!spellAttributes.IsReady || spellAttributes.Caster == null)
        {
            FinishEffect();
            return;
        }

        _caster = spellAttributes.Caster;
        GlobalPosition = _caster.Position;
        _caster.IsDisplaced = true;
        _isDashing = true;

        _caster.StopMovement();
        SetTravelPoint(spellAttributes.Position, spellAttributes.SpellRange);

        _movement = _dashDirection * DashSpeed;
        _previousDashDistance = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isDashing)
        {
            base._PhysicsProcess(delta);
            return;
        }

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
            FinishEffect();
            QueueFree();
            return;
        }

        GlobalPosition = _caster.Position;
        _caster.Velocity = _movement * 100;
        _caster.MoveAndSlide();
    }

    private void SetTravelPoint(Vector2 position, float spellRange)
    {
        _dashDirection = (position - _caster.Position).Normalized();
        _dashTarget = _caster.Position + _dashDirection * spellRange;

        var result = GetWorld2D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters2D
        {
            From = _caster.Position,
            To = _dashTarget,
            Exclude = new Godot.Collections.Array<Rid> { _caster.GetRid() },
            CollisionMask = 1
        });

        if (result.Count > 0)
        {
            _dashTarget = result["position"].AsVector2() - _dashDirection * 30;
        }
    }

    private bool CheckDistance(float distance)
    {
        if (_previousDashDistance != 0f && _previousDashDistance < distance)
        {
            return false;
        }

        _previousDashDistance = distance;
        return true;
    }
}
