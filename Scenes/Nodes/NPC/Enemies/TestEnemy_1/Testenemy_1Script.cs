using Godot;
using Godot.Collections;
using System;

public partial class Testenemy_1Script : HostileNPC
{

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
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        CheckForPlayerInSight();

        if (onAlert)
        { 
            MoveToRangeOrLastKnownPosition();
        }

        if (CheckForPlayerInSight())
            {
                if (onAlert)
                {
                    StopMovement();
                }
            }
            else
            {
                if (!onAlert)
                {
                    StopMovement();
                }
                {


                }
            }
    }
}
