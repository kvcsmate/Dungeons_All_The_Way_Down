using Godot;
using System;
using System.Threading;

public partial class FireboltEffect : SpellEffect
{
    public Vector2 Direction;
    public int Damage;
    public float Speed;
    public float MaxDistance;
    CollisionObject2D _collider;

    private Vector2 _startPosition;
    private float _distanceTraveled = 0f;

    public override void _Ready()
    {
        base._Ready();
        _startPosition = GlobalPosition;
        //_collider = this.GetParent<StaticBody2D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Rotation = Direction.Angle();
        Vector2 movement = Direction * Speed * (float)delta;
  
        _distanceTraveled += movement.Length();

        if (Disposable)
        {
            movement = Vector2.Zero;
        }
        else
        {
            if (_distanceTraveled >= MaxDistance)
            {
                Disposable = true;
            }

            var collision = MoveAndCollide(movement);
            if (collision != null)
            {
                GD.Print("HIT");
                //Velocity = Velocity.Bounce(collision.GetNormal());

                if (collision.GetCollider().HasMethod("OnHit"))
                {
                    
                    collision.GetCollider().Call("OnHit", Damage);
                }
                Disposable = true;
            }
        }
       
        //Position += movement;

    }
}
