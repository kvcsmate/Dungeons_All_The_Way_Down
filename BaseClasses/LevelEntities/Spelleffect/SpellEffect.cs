using Godot;
using System;

public abstract partial class SpellEffect : LevelEntity
{
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
}
