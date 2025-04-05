using Godot;
using System;

public partial class Testenemy_1Script : NPC
{
    //[Export]
    //new string SpriteName = "AnimatedSprite2D";

    private NavigationAgent2D _navigationAgent;

    private float _movementSpeed = 5.0f;
    //private Vector2 _movementTargetPosition = new Vector2(70.0f, 226.0f);
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
        _target =  GetParent().GetNode<CharacterBody2D>("Player");
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        _navigationAgent.PathDesiredDistance = 1.0f;
        //_navigationAgent.TargetDesiredDistance = 4.0f;

        // Make sure to not await during _Ready.
        //Callable.From(ActorSetup).CallDeferred();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        _navigationAgent.TargetPosition = _target.Position;

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        Vector2 currentAgentPosition = GlobalTransform.Origin;
        Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();


        Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * _movementSpeed;
        MoveAndSlide();
    }

    //private async void ActorSetup()
    //{
    //    // Wait for the first physics frame so the NavigationServer can sync.
    //    await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

    //    // Now that the navigation map is no longer empty, set the movement target.
    //    MovementTarget = _movementTargetPosition;
    //}


}
