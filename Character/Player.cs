using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBody2D
{
    [Export] public int Speed = 400;
    private Vector2 _targetPosition;
    private bool _isMoving = false;

    private bool _isDisplaced;

    public bool IsDisplaced
    {
        get { return _isDisplaced; }
        set { _isDisplaced = value; }
    }

    public Sprite2D PlayerSprite;

    private Node2D _currentIndicator;

    public String IndicatorLocation = "res://HUD//Indicator//Indicator.tscn";

    public PackedScene FireballScene = (PackedScene)GD.Load("res://Spells//Fireball//Fireball.tscn");
    public PackedScene DashScene = (PackedScene)GD.Load("res://Spells//Dash//Dash.tscn");

    private Spell _fireballSpell;
    private Spell _dashSpell;

    public PackedScene IndicatorScene;

    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;

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
                    case StateEnum.Run: StateMachine.Travel("Running"); break;
                    default: StateMachine.Travel("Idle"); break;
                }

            }
        }
    }

    public enum StateEnum
    {
        Idle,
        Run
    }


    public override void _Ready()
    {

        IndicatorScene = (PackedScene)GD.Load(IndicatorLocation);

        PlayerSprite = this.GetNode<Sprite2D>("Sprite");

        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        // get State machine 
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

        CurrentState = (int)StateEnum.Idle;
        _targetPosition = Position;

        _fireballSpell = (Spell)FireballScene.Instantiate();
        AddChild(_fireballSpell);

        _dashSpell = (Spell)DashScene.Instantiate();
        AddChild(_dashSpell);

        
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
        {
            _targetPosition = GetGlobalMousePosition();
            _isMoving = true;

            if (_currentIndicator != null)
            {
                _currentIndicator.QueueFree();
                _currentIndicator = null;
            }

            _currentIndicator = (Node2D)IndicatorScene.Instantiate();
            _currentIndicator.Position = _targetPosition;
            GetParent().AddChild(_currentIndicator);
        }
        else
        {
            if (Input.IsActionJustPressed("Q"))
            {
                Vector2 spellPosition = GetGlobalMousePosition();
                _fireballSpell.Cast(spellPosition);
            }
            if (Input.IsActionJustPressed("F"))
            {
                Vector2 spellPosition = GetGlobalMousePosition();
                _dashSpell.Cast(spellPosition);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving && !IsDisplaced)
        {
            Vector2 direction = (_targetPosition - Position).Normalized();
            Vector2 movement = direction * Speed * (float)delta;

            if (Position.DistanceTo(_targetPosition) <= .125 )
            {
                StopMovement(); 
            }
            else
            {
                this.Velocity = movement;
                MoveAndSlide();
            }
        }
        SetFlipDirection();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (Velocity == Vector2.Zero)
        {
            CurrentState = StateEnum.Idle;
            // GD.Print(StateMachine.GetCurrentNode());
        }
        if (Velocity != Vector2.Zero)
        {
            CurrentState = StateEnum.Run;
            //GD.Print(StateMachine.GetCurrentNode());
        }
    }

    private void SetFlipDirection()
    {
        if (Velocity.X > 0)
        {
            PlayerSprite.FlipH = false;
        }
        else if (Velocity.X < 0)
        {
            PlayerSprite.FlipH = true;
        }
    }
    public void StopMovement()
    {
        //Position = _targetPosition;
        _isMoving = false;
        this.Velocity = Vector2.Zero;

        if (_currentIndicator != null)
        {
            _currentIndicator.QueueFree();
            _currentIndicator = null;
        }
    }
}