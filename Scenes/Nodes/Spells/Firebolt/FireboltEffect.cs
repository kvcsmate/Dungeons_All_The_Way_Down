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
    public Node2D Caster;
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
        this.AddCollisionExceptionWith(Caster);
        
        Rotation = Direction.Angle();
        Vector2 movement = Direction * Speed * (float)delta;
  
        _distanceTraveled += movement.Length();


        if (Disposable)
        {
            movement = Vector2.Zero;
        }


        if(!Disposable)
        {
            if (_distanceTraveled >= MaxDistance)
            {
                Disposable = true;
                GD.Print("Max distance reached, disposing Firebolt.");
            }

            var collision = MoveAndCollide(movement);
            
            if (collision != null && collision.GetCollider() != Caster && collision.GetCollider() != this)
            {
                GD.Print("HIT");
                //Velocity = Velocity.Bounce(collision.GetNormal());

                if (collision.GetCollider().HasMethod("OnHit"))
                {
                    
                    collision.GetCollider().Call("OnHit", Damage);
                }
                Disposable = true;
            }
            if (collision != null)
            {
                PrintCollider(collision);
            }
        }
       
        //Position += movement;

    }
    private void PrintCollider(KinematicCollision2D collision)
    {
                        if (collision.GetCollider() is Node colliderNode)
                {
                    var owner = colliderNode.GetOwner();
                    string scenePath = !string.IsNullOrEmpty(owner?.SceneFilePath) ? owner.SceneFilePath : colliderNode.SceneFilePath;
                    string sceneName = !string.IsNullOrEmpty(scenePath)
                        ? System.IO.Path.GetFileName(scenePath)
                        : colliderNode.Name;
                    GD.Print($"Hit scene: {sceneName} (node: {colliderNode.Name})");
                }
                else
                {
                    GD.Print(collision.GetCollider());
                }
    }

}
