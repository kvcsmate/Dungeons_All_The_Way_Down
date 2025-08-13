using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonsAlltheWayDown.Scenes.Nodes.Character;
using Godot;
using Godot.Collections;

public abstract partial class HostileNPC : Character
{
   
    [Export]
    public float SightRadius = 1000.0f;


    protected bool onAlert = false;

    protected NavigationAgent2D _navigationAgent;
    private Player _target;

    public Sprite2D CharacterSprite;

    private EnemyHealthBar _healthBar;

    private StateEnum _currentState;
    private Vector2 lastKnownPosition;


    public StateEnum CurrentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState != value)
            {
                _currentState = value;

                switch (CurrentState)
                {
                    case StateEnum.Run: StateMachine.Travel("Run"); break;
                    case StateEnum.Death: StateMachine.Travel("Death"); break;
                    default: StateMachine.Travel("Idle"); break;
                }

            }
        }
    }


    public enum StateEnum
    {
        Idle,
        Run,
        Death
    }

    public void DeathCheck()
    {
        if (Health <= 0)
        {
            Disposable = true;
        }
    }

    public override void _Ready()
    {

        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        //AnimationTree.AnimationStarted += OnAnimationFinished;
        //get State machine
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
        _target = GetParent().GetNode<Player>("Player");
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.PathDesiredDistance = 10.0f;
        lastKnownPosition = GlobalPosition;
        //GD.Print(_navigationAgent);

        LoadSprite(GetSpriteName());

        _healthBar = GetNodeOrNull<EnemyHealthBar>("EnemyHealthBar");
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(Health, MaxHealth);
        }

    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_navigationAgent.IsNavigationFinished())
        {
            Vector2 currentAgentPosition = GlobalTransform.Origin;
            Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();

            Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * MovementSpeed;
            MoveAndSlide();

            //SetFlipDirection();
        }
    }

    public void MoveToPlayerOrLastKnownPosition()
    {
        if (CheckForPlayerInSight())
        {
            MoveToPlayer();
        }
        else
        {
            MoveToLastKnownPosition();
        }
    }

    public void MoveToRangeOrLastKnownPosition()
    {
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) > SightRadius)
        {
            MoveUntilPlayerInSight();
        }
        else
        { 
            MoveToLastKnownPosition();
        }
    }

    private void MoveToPlayer()
    {
        _navigationAgent.TargetPosition = _target.Position;
    }

    private void MoveToLastKnownPosition()
    {
        _navigationAgent.TargetPosition = lastKnownPosition;
    }

    private void MoveUntilPlayerInSight()
    {
        if (!CheckForPlayerInSight())
        {
            //GD.Print(_target);
            //GD.Print(_target.playerSight);
            _navigationAgent.TargetPosition = GetClosestSightPoint(_target.GlobalPosition);
            lastKnownPosition = _target.GlobalPosition;
        }
        else
        {
            StopMovement();
        }
    }

    public Vector2 GetClosestSightPoint(Vector2 position)
    {
        List<Vector2> SightMatrix = _target.GetNode<PlayerSight>("PlayerSight").SightMatrix;
        Vector2 closestPoint = SightMatrix[0];
        float closestDistance = position.DistanceTo(closestPoint);

            foreach (var point in SightMatrix)
            {
                _navigationAgent.TargetPosition = point;
                float distance = _navigationAgent.DistanceToTarget();
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
            _navigationAgent.TargetPosition = closestPoint;
            return closestPoint;
        }


    public static Vector2 ClosestPointOnSegment(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 AB = B - A;
        Vector2 AC = C - A;

        float t = AC.Dot(AB) / AB.LengthSquared();
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        return A + AB * t;
    }

    public override void StopMovement()
    {
        _navigationAgent.TargetPosition = GlobalPosition;
        Velocity = Vector2.Zero;
        MoveAndSlide();
    }

    private void SetFlipDirection()
    {
        if (Velocity.X < 0)
        {
            CharacterSprite.FlipH = true;
        }
        else if (Velocity.X > 0)
        {
            CharacterSprite.FlipH = false;
        }
    }


    protected bool CheckForPlayerInSight()
    {
        // Check radius
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) > SightRadius)
            return false;

        // Raycast to player
        var spaceState = GetWorld2D().DirectSpaceState;

        var result = spaceState.IntersectRay(
            new PhysicsRayQueryParameters2D
            {
                From = Position,
                To = _target.Position,
                CollisionMask = 2, // Adjust as needed for your collision layers
                Exclude = new Array<Rid> { this.GetRid() } // Exclude self from raycast
            }
        );

        // If nothing blocks the ray or the first hit is the player, player is in sight
        if (result.Count == 0)
        {
            onAlert = true;
            return true;
        }

        Node2D collider = result["collider"].As<Node2D>();
        //GD.Print("Collider detected: " + collider.Name);
        if (collider is Player)
        {
            onAlert = true;
            return true;
        }
        else
        {
            return false;
        }
    }
    public string GetSpriteName()
    {
        if (SpriteName is not null)
        {
            return SpriteName;
        }
        else
        {
            foreach (var item in this.GetChildren())
            {

                if (item is Sprite2D || item is AnimatedSprite2D)
                {
                    return item.Name;
                }
            }
        }

        return "Sprite2D";
    }

    public abstract void LoadSprite(string SpriteName);
    //{
    //    CharacterSprite = this.GetNode<Sprite2D>(SpriteName);
    //}


    public virtual void OnHit(int Damage)
    {
        Health -= Damage;
        if (Health < 0)
            Health = 0;

        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(Health, MaxHealth);
        }

        GD.Print(Health);

        DeathCheck();
    }

    public override void AnimHandler()
    {
        if (Disposable)
        {
            CurrentState = StateEnum.Death;
        }
    }
}

