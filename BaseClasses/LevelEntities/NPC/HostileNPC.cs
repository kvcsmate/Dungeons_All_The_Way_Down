using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    private NavigationAgent2D _navigationAgent;
    private Node2D _target;

    public Sprite2D CharacterSprite;

    private StateEnum _currentState;
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
        _target = GetParent().GetNode<CharacterBody2D>("Player");
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.PathDesiredDistance = 10.0f;
        GD.Print(StateMachine + this.Name);

        LoadSprite(GetSpriteName());



    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (PlayerInSight())
        {
            NPCAlert();
            
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

    private void NPCAlert()
    {
        // Here, decide how the NPC should react when the player is in sight.
    }

    private void MoveToPlayer()
    {
        _navigationAgent.TargetPosition = _target.Position;
    }

    private void MoveUntilPlayerInSight()
    {
        if (!PlayerInSight())
        {
            MoveToPlayer();
        }
        else
        { 
            _navigationAgent.TargetPosition = GlobalPosition;
            Velocity = Vector2.Zero;
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
            return true;

        Node2D collider = result["collider"].As<Node2D>();
        //GD.Print("Collider detected: " + collider.Name);
        return collider == _target;
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

