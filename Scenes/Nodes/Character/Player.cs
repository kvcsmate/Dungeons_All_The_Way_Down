using DungeonsAlltheWayDown.AbilitySystem;
using DungeonsAlltheWayDown.Scenes.Nodes.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Player : CharacterBody2D
{
    [Export] public int Speed = 400;
    [Export] public int MaxHealth = 100;
    [Export] public int Health = 100;

    private PlayerHealthBar _healthBar;
    private SpellHUD _spellHUD;
    private Vector2 _targetPosition;
    private bool _isMoving = false;

    private bool _isDisplaced;

    private Node2D _currentIndicator;

    private StateEnum _currentState;

    public NavigationAgent2D _navigationAgent;

    public PlayerSight playerSight;

    SpellLoader spellLoader;

    SpellBook spellBook;

    AbilityInputMap abilityInputMap;

    public enum MovementMode
    {
        Click,
        Joystick
    }

    [Export]
    public MovementMode CurrentMovementMode = MovementMode.Joystick;
    public bool IsDisplaced
    {
        get { return _isDisplaced; }
        set { _isDisplaced = value; }
    }

    public Vector2 MovementTarget
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }

    public Sprite2D PlayerSprite;

    public String IndicatorLocation = "res://Scenes//Nodes//HUD//Indicator//Indicator.tscn";

    /*
    public PackedScene FireballScene = (PackedScene)GD.Load("res://Scenes//Nodes//Spells//Fireball//Fireball.tscn");
    public PackedScene DashScene = (PackedScene)GD.Load("res://Scenes//Nodes//Spells//Dash//Dash.tscn");
    public PackedScene FireBoltScene = (PackedScene)GD.Load("res://Scenes//Nodes//Spells//FireBolt//FireBolt.tscn");

    private Spell _fireballSpell;
    private Spell _dashSpell;
    private Spell _fireboltSpell;
    */
    public PackedScene IndicatorScene;

    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;
    private Vector2 rotationvector;
    private Vector2 testvector;


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
                    case StateEnum.Run: StateMachine.Travel("Running"); break;
                    default: StateMachine.Travel("Idle"); break;
                }

            }
        }
    }

    public string MarkerLocation = "res://Scenes/Nodes/HUD/Marker.tscn";


    public PackedScene MarkerScene;

    private List<Node2D> testMarkers;

    public enum StateEnum
    {
        Idle,
        Run
    }


    private FacingDirection _facing = FacingDirection.Front;
    private Vector2 _aimDirection;


    public FacingDirection Facing
    {
        get { return _facing; }
        set
        {
            if (_facing != value)
            {
                _facing = value;
                // Use StateMachine to travel to the correct facing state if needed
                // (Optional: you can use this for future expansion)
            }
        }
    }

    public enum FacingDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    public override void _Ready()
    {
        GD.Print(OS.GetExecutablePath());
        spellLoader = new SpellLoader();
        spellBook = new SpellBook(spellLoader, this);
        abilityInputMap = new AbilityInputMap(spellBook);
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.PathDesiredDistance = 1.0f;

        IndicatorScene = (PackedScene)GD.Load(IndicatorLocation);

        PlayerSprite = this.GetNode<Sprite2D>("Sprite");

        _healthBar = GetNode<PlayerHealthBar>("PlayerHealthBar");
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(Health, MaxHealth);
        }

        _spellHUD = GetNode<SpellHUD>("SpellHUD");
        if (_spellHUD != null)
        {
            _spellHUD.SpellBook = spellBook;

        }

        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        // get State machine 
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

        CurrentState = (int)StateEnum.Idle;
        _targetPosition = Position;

        //playerSight = new PlayerSight(30,100,this);
        //this.AddChild(playerSight);
        //playerSight.Position = Position;

        // DEBUG

        GD.Print(spellLoader.SpellDirectory);
        UpdateSpellbook(0, "Firebolt");
        UpdateSpellbook(1, "Fireball");
        UpdateSpellbook(2, "Dash");

    }
    public override void _PhysicsProcess(double delta)
    {
        CharacterMovement(delta);

        OnHit(1); //for testing purposes, remove later
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentMovementMode == MovementMode.Click)
        {
            HandlePointClickInput(@event);
        }

        if (@event is InputEvent keyevent && keyevent.IsPressed())
        {
            Spell.SpellParams spellParams = new Spell.SpellParams()
            {
                Position = GetAimDirection(),
                Player = this
            };
            abilityInputMap.HandleInput(@event, spellParams);
        }
    }

    private void HandlePointClickInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
        {
            Vector2 clickedPosition = GetGlobalMousePosition();

            _navigationAgent.TargetPosition = clickedPosition;
            if (!_navigationAgent.IsTargetReachable())
            {
                var navMap = _navigationAgent.GetNavigationMap();
                clickedPosition = NavigationServer2D.MapGetClosestPoint(navMap, clickedPosition);
                _navigationAgent.TargetPosition = clickedPosition;
            }

            _targetPosition = clickedPosition;
            _isMoving = true;

            CreateIndicator(_targetPosition);
        }
    }


        public void CreateIndicator(Vector2 position)
    {
        if (_currentIndicator != null)
        {
            _currentIndicator.QueueFree();
            _currentIndicator = null;
        }

        _currentIndicator = (Node2D)IndicatorScene.Instantiate();
        _currentIndicator.Position = position;
        GetParent().AddChild(_currentIndicator);
    }

    private Vector2 GetAimDirection()
    {
        if (CurrentMovementMode == MovementMode.Click)
        {
            return GetGlobalMousePosition();
        }
        else
        {
            return GetJoystickTargetDirection();
        }
    }
    private void CharacterMovement(double delta)
    {
        if (CurrentMovementMode == MovementMode.Click)
        {
            ClickMovement(delta);
        }
        else
        {
            JoystickMovement(delta);
        }

        HandleAnimation();
    }

    private void ClickMovement(double delta)
    {
        _navigationAgent.TargetPosition = _targetPosition;

        if (_isMoving && !IsDisplaced)
        {
            Vector2 currentAgentPosition = GlobalTransform.Origin;
            Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();

            if (Position.DistanceTo(_targetPosition) <= 20)
            {
                StopMovement();
            }
            else
            {
                Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
                MoveAndSlide();
            }
        }
    }

    private void JoystickMovement(double delta)
    {
        Vector2 direction = new Vector2(
            Input.GetJoyAxis(0, JoyAxis.LeftX),
            Input.GetJoyAxis(0, JoyAxis.LeftY)
        );

        if (Mathf.Abs(direction.X) < 0.1f)
            direction.X = 0;
        if (Mathf.Abs(direction.Y) < 0.1f)
            direction.Y = 0;

        Velocity = direction * Speed;
        MoveAndSlide();
        _targetPosition = Position;
        _isMoving = direction != Vector2.Zero;
    }

    private void HandleAnimation()
    {
        // Determine facing direction based on velocity
        if (Velocity.Length() == 0)
        {
            // Keep previous facing direction
        }
        else if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y))
        {
            Facing = Velocity.X > 0 ? FacingDirection.Right : FacingDirection.Left;
        }
        else
        {
            Facing = Velocity.Y > 0 ? FacingDirection.Front : FacingDirection.Back;
        }

        // Set state and play correct animation
        if (Velocity == Vector2.Zero)
        {
            CurrentState = StateEnum.Idle;
            PlayDirectionalAnimation("Idle");
        }
        else
        {
            CurrentState = StateEnum.Run;
            PlayDirectionalAnimation("Running");
        }
    }

    private void PlayDirectionalAnimation(string baseName)
    {
        string anim = baseName + "_";
        switch (Facing)
        {
            case FacingDirection.Front: anim += "Front"; break;
            case FacingDirection.Back: anim += "Back"; break;
            case FacingDirection.Left: anim += "Left"; break;
            case FacingDirection.Right: anim += "Right"; break;
        }

        // Use StateMachine to travel to the correct animation node
        if (StateMachine != null)
        {
            StateMachine.Travel(anim);
        }
    }
    public void StopMovement()
    {

        //!_navigationAgent.IsNavigationFinished()
        if (CurrentMovementMode == MovementMode.Click)
        {

            _targetPosition = Position; // Reset target position to current position
        }

        _isMoving = false;
        this.Velocity = Vector2.Zero;

        if (_currentIndicator != null)
        {
            _currentIndicator.QueueFree();
            _currentIndicator = null;
        }
    }

    public void OnHit(int damage)
    {
        Health -= damage;
        if (Health < 0)
            Health = 0;

        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(Health, MaxHealth);
        }
    }
    public void UpdateSpellbook(int index, string spellName)
    {
        if (spellBook != null)
        {
            spellBook.Update(index, spellName);
            _spellHUD.UpdateSpellHUD(index);
        }
        else
        {
            GD.PrintErr("SpellBook is not initialized.");
        }
    }

    private Vector2 GetJoystickTargetDirection()
    {
       Vector2 direction = new Vector2(
			Input.GetJoyAxis(0, JoyAxis.RightX),
			Input.GetJoyAxis(0, JoyAxis.RightY));
            
		if (direction.Length() == 0)
        {
            direction = new Vector2(
                Input.GetJoyAxis(0, JoyAxis.LeftX),
                Input.GetJoyAxis(0, JoyAxis.LeftY));

        }
        return direction + this.GlobalPosition;
    }
}

