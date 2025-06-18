using Godot;
using System;

public partial class Dash : Spell
{
    [Export] public float DashSpeed = 100f;
    private Vector2 _dashDirection;
    private float _dashDistance;
    private Vector2 _dashTarget;
    private bool _isDashing = false;
    Player _player;
    Vector2 movement;

    float previousDashDistance = 0f;

    public override void _Ready()
    {
        _player = this.GetParent<Player>();
    }

    public override void Cast(SpellParams p)
    {
        _player = p.Player;
        _player.IsDisplaced = true;
        _isDashing = true;

        // Clear the navigation agent's next point to stop pathfinding during dash
        _player.StopMovement();
        SetTravelPoint(p.Position);

        movement = _dashDirection * DashSpeed;
        previousDashDistance = 0f; // Reset previous dash distance
    }

    private void SetTravelPoint(Vector2 position)
    {
        _dashDirection = (position - _player.Position).Normalized();
        _dashDistance = SpellRange;

        // Raycast to check for obstacles
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
            //GD.Print("Obstacle detected at: " + result["position"]);
            _dashTarget = ((Vector2)result["position"]) - _dashDirection * 30;
            // Adjust the target position to stop before the obstacle :
            //  _dashDirection moves the target position back by x pixels
        }
        //_player.CreateIndicator(_dashTarget); // Debbugging the end point of the dash
    }


    public override void _PhysicsProcess(double delta)
    {
        // Check if the player is dashing
        if (_isDashing)
        {
            // Calculate the remaining distance to the target
            float remainingDistance = _player.Position.DistanceTo(_dashTarget);

            if (!CheckDistance(remainingDistance))
            {
                _player.Position = _dashTarget; // Snap to the target position if the dash went over the target
            }

            if (remainingDistance <= 100)
            {
                _isDashing = false;

                _player.Velocity = Vector2.Zero; // _dashDirection * remainingDistance;
                _player.MoveAndSlide(); // Finish the dash movement 
                _player.IsDisplaced = false;

                StartCooldown();
            }
            else
            {
                if (Engine.GetPhysicsFrames() % 3 == 0)
                {
                    var spellEffect = (Node2D)SpellEffectScene.Instantiate();
                    GetTree().Root.AddChild(spellEffect);
                    spellEffect.GlobalPosition = _player.Position;
                }
                _player.Velocity = movement * 100; // Move the player in the dash direction
                _player.MoveAndSlide();
            }
        }
    }
    private bool CheckDistance(float distance)
    {
        GD.Print("Distance: " + distance + " Previous Distance: " + previousDashDistance);
        if (previousDashDistance != 0f && previousDashDistance < distance)
        {
            GD.Print("Dash distance too far, resetting dash.");
            // If the previous dash distance is less than the current distance, it means the player is trying to dash too far
            return false;
        }
        else
        {
            // If the previous dash distance is greater than or equal to the current distance, update the previous distance
            previousDashDistance = distance;
            return true;
        }
    }

}
