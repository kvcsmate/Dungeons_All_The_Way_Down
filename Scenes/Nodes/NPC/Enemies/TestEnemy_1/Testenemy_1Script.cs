using Godot;
using Godot.Collections;
using System;

public partial class Testenemy_1Script : NPC
{
    [Export]
    public float MovementSpeed = 500.0f;
    [Export]
    public float SightRadius = 1000.0f;
    [Export]
    public float SightRaycastMargin = 2.0f; // Small margin to avoid self-collision

    private NavigationAgent2D _navigationAgent;
    private Node2D _target;
    public new AnimatedSprite2D CharacterSprite;

    public override void LoadSprite(string SpriteName)
    {
        CharacterSprite = this.GetNode<AnimatedSprite2D>(SpriteName);
    }

    public Vector2 MovementTarget
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }

    public override void _Ready()
    {
        base._Ready();
        _target = GetParent().GetNode<CharacterBody2D>("Player");
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        _navigationAgent.PathDesiredDistance = 1.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (PlayerInSight())
        {
            _navigationAgent.TargetPosition = _target.Position;
        }
        else
        {
            // Stop moving if player is not in sight
            _navigationAgent.TargetPosition = GlobalPosition;
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        if (!_navigationAgent.IsNavigationFinished())
        {
            Vector2 currentAgentPosition = GlobalTransform.Origin;
            Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();

            Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * MovementSpeed;
            MoveAndSlide();
        }
    }

    private bool PlayerInSight()
    {
        // Check radius
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) > SightRadius)
            return false;

        // Raycast to player
        var spaceState = GetWorld2D().DirectSpaceState;

        var exclude = new Array<Rid>{ this.GetRid() }; // Exclude self from raycast
        var result = spaceState.IntersectRay(
            new PhysicsRayQueryParameters2D
            {
                From = Position,
                To = _target.Position,
                CollisionMask = 2, // Adjust as needed for your collision layers
                Exclude = exclude
            }
        );

        // If nothing blocks the ray or the first hit is the player, player is in sight
        if (result.Count == 0)
            return true;

        Node2D collider = result["collider"].As<Node2D>();
        GD.Print("Collider detected: " + collider.Name);
        return collider == _target;
    }
}
