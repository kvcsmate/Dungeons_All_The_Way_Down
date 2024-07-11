using Godot;
using System;

public partial class FireballEffect : Node2D
{
    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    // if the animation is started, then the _Ready is already done
    public void OnAnimationFinished(StringName animName)
    {
        this.Dispose();
    }
}
