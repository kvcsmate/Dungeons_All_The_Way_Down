// Shared NPC foundation.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public abstract partial class HostileNPC : Character
{
   
    [Export]
    public float SightRadius = 1000.0f;

    [Export]
    public float PreferredAttackRange = 650.0f;

    [Export]
    public float AttackRangeTolerance = 75.0f;

    [Export]
    public uint SightCollisionMask = 1;

    [Export]
    public int SightPositionSampleCount = 24;


    protected bool onAlert = false;

    protected NavigationAgent2D _navigationAgent;
    protected Player _target;

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
            MovementSpeed = 0;
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
        bool canSeePlayer = CheckForPlayerInSight();

        // If the player is visible, keep the enemy at its preferred firing distance.
        // If sight is blocked, search for a nearby navigable position that can see the player.
        if (canSeePlayer)
        {
            lastKnownPosition = _target.GlobalPosition;

            if (IsPlayerInPreferredAttackRange())
            {
                StopMovement();
                return;
            }

            MoveToAttackRange();
        }
        else
        {
            MoveUntilPlayerInSight();
        }
    }

    private void MoveToPlayer()
    {
        _navigationAgent.TargetPosition = _target.GlobalPosition;
    }

    private void MoveToLastKnownPosition()
    {
        _navigationAgent.TargetPosition = lastKnownPosition;
    }

    private void MoveUntilPlayerInSight()
    {
        if (TryGetClosestVisibleAttackPosition(_target.GlobalPosition, out Vector2 sightPosition))
        {
            SetNavigationTarget(sightPosition);
        }
        else
        {
            MoveToLastKnownPosition();
        }
    }

    private void MoveToAttackRange()
    {
        Vector2 directionFromPlayer = (_target.GlobalPosition - GlobalPosition).Normalized();
        if (directionFromPlayer == Vector2.Zero)
        {
            directionFromPlayer = Vector2.Right;
        }

        Vector2 desiredPosition = _target.GlobalPosition - directionFromPlayer * PreferredAttackRange;
        desiredPosition = GetClosestNavigationPoint(desiredPosition);
        SetNavigationTarget(desiredPosition);
    }

    public bool IsPlayerInPreferredAttackRange()
    {
        return GlobalPosition.DistanceTo(_target.GlobalPosition) <= PreferredAttackRange + AttackRangeTolerance;
    }

    private bool TryGetClosestVisibleAttackPosition(Vector2 targetPosition, out Vector2 bestPosition)
    {
        bestPosition = GlobalPosition;

        Rid navMap = _navigationAgent.GetNavigationMap();

        // Sample rings around the player, then snap each sample to the navigation map.
        // A candidate is useful only if it stays inside sight range, has direct line of sight,
        // and can be reached through the nav map.
        float[] sampleRadii =
        {
            PreferredAttackRange,
            Mathf.Max(PreferredAttackRange - AttackRangeTolerance, PreferredAttackRange * 0.75f),
            Mathf.Min(SightRadius * 0.9f, PreferredAttackRange + AttackRangeTolerance),
            SightRadius * 0.5f
        };

        float bestScore = float.MaxValue;
        foreach (float radius in sampleRadii)
        {
            if (radius <= 0.0f)
            {
                continue;
            }

            for (int i = 0; i < SightPositionSampleCount; i++)
            {
                float angle = Mathf.Tau * i / SightPositionSampleCount;
                Vector2 samplePosition = targetPosition + Vector2.FromAngle(angle) * radius;
                Vector2 navigationPosition = GetClosestNavigationPoint(samplePosition);

                if (navigationPosition.DistanceTo(targetPosition) > SightRadius)
                {
                    continue;
                }

                if (!HasLineOfSightToTarget(navigationPosition, targetPosition))
                {
                    continue;
                }

                float pathLength = GetNavigationPathLength(navMap, GlobalPosition, navigationPosition);
                if (pathLength < 0.0f)
                {
                    continue;
                }

                // Prefer short paths, but slightly favor spots close to the desired attack range.
                float rangeDifference = Mathf.Abs(navigationPosition.DistanceTo(targetPosition) - PreferredAttackRange);
                float score = pathLength + rangeDifference * 0.5f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPosition = navigationPosition;
                }
            }
        }

        return bestScore < float.MaxValue;
    }

    private Vector2 GetClosestNavigationPoint(Vector2 position)
    {
        return NavigationServer2D.MapGetClosestPoint(_navigationAgent.GetNavigationMap(), position);
    }

    private float GetNavigationPathLength(Rid navMap, Vector2 from, Vector2 to)
    {
        Vector2[] path = NavigationServer2D.MapGetPath(navMap, from, to, true, _navigationAgent.NavigationLayers);

        if (path.Length < 2)
        {
            return from.DistanceTo(to) <= _navigationAgent.TargetDesiredDistance ? 0.0f : -1.0f;
        }

        float length = 0.0f;
        for (int i = 1; i < path.Length; i++)
        {
            length += path[i - 1].DistanceTo(path[i]);
        }

        return length;
    }

    private void SetNavigationTarget(Vector2 targetPosition)
    {
        if (_navigationAgent.TargetPosition.DistanceTo(targetPosition) > 5.0f)
        {
            _navigationAgent.TargetPosition = targetPosition;
        }
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

        bool hasLineOfSight = HasLineOfSightToTarget(GlobalPosition, _target.GlobalPosition);
        if (hasLineOfSight)
        {
            onAlert = true;
        }

        return hasLineOfSight;
    }

    private bool HasLineOfSightToTarget(Vector2 from, Vector2 to)
    {
        var spaceState = GetWorld2D().DirectSpaceState;

        var result = spaceState.IntersectRay(
            new PhysicsRayQueryParameters2D
            {
                From = from,
                To = to,
                CollisionMask = SightCollisionMask,
                Exclude = new Array<Rid> { this.GetRid() } // Exclude self from raycast
            }
        );

        if (result.Count == 0)
        {
            return true;
        }

        Node2D collider = result["collider"].As<Node2D>();
        return collider is Player;
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
