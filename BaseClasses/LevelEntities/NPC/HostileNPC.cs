using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonsAlltheWayDown.Scenes.Nodes.Character;
using Godot;
using Godot.Collections;

public abstract partial class HostileNPC : LevelEntity
{
    [Export]
    public int Health = 100;

    [Export]
    public string Title;

    [Export]
    public string SpriteName;

    [Export]
    public float MovementSpeed = 500.0f;
    [Export]
    public float SightRadius = 1000.0f;

    [Export]
    public Godot.Collections.Array<PackedScene> Spells;

    protected bool onAlert = false;

    protected NavigationAgent2D _navigationAgent;
    private Player _target;

    public Sprite2D CharacterSprite;

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

            SetFlipDirection();
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
            GD.Print(_target);
            //GD.Print(_target.playerSight);
            GD.Print(_target.GetNode<PlayerSight>("PlayerSight").GetClosestSightPoint(Position));
            _navigationAgent.TargetPosition = _target.GetNode<PlayerSight>("PlayerSight").GetClosestSightPoint(Position);
        }
        else
        {
            StopMovement();
        }
    }

    protected void StopMovement()
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

