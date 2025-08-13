using Godot;
using System;

public abstract partial class SpellEffect : LevelEntity
{
    public enum CallerType
    {
        NPC,
        Player
    }

    public Node2D _callerEntity;

    public CallerType Caller;

    public override void AnimHandler()
    {
        if (Disposable)
        {
            if (StateMachine.GetCurrentNode() != "Effect")
            {
                StateMachine.Travel("Effect");
            }
        }
    }
    public override void _Ready()
    {
        base._Ready();
    }
}
