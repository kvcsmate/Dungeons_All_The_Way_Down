using Godot;
using System;

public partial class FireBoltEffect : SpellEffect
{
    public Vector2 Direction;
    public int Damage;
    public float Speed;
    [Export] public float MaxDistance = 500f;
    CollisionObject2D _collider;

    private Vector2 _startPosition;
    private float _distanceTraveled = 0f;

    public override void OnInit()
    {
        
        _startPosition = GlobalPosition;
        //_collider = this.GetParent<StaticBody2D>();
    }

    public override void OnFrame(double delta)
    {
        GD.Print(Direction.Angle());
        Rotation = Direction.Angle();
        Vector2 movement = Direction * Speed * (float)delta;
        Position += movement;
        _distanceTraveled += movement.Length();

        // Handle collision detection
        //var spaceState = GetWorld2D().DirectSpaceState;

        //var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + movement);
        //query.Exclude = new Godot.Collections.Array<Rid> { _collider.GetRid() };
        //var collision = spaceState.IntersectRay(query);

        //if (collision.Count > 0)
        //{
        //    var collider = collision["collider"] as Node2D;
        //    if (collider is IDamageable damageable)
        //    {
        //        damageable.TakeDamage(Damage);
        //    }
        //    QueueFree(); // Destroy the fire bolt after collision
        //}

        // Check if the fire bolt has traveled the maximum distance
        if (_distanceTraveled >= MaxDistance)
        {
            QueueFree(); // Destroy the fire bolt after exceeding the maximum distance
        }
    }
}
