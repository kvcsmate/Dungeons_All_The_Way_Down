using Godot;
using System;

public abstract partial class LevelEntity : CharacterBody2D
{
    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;

    public bool Disposable = false;  // is set when the entity is ready to be deleted. 
                                    // Needed, because we don't want to dispose entities mid-animation, only after
                                    // they have finished their animation


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        //AnimationTree.AnimationStarted += OnAnimationFinished;
        //get State machine
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {

        AnimHandler();
        GarbageCollection();
    }

    public abstract void AnimHandler();
    //{
    //    if (Disposable)
    //    {
    //        if (StateMachine.GetCurrentNode() != "Effect")
    //        {
    //            StateMachine.Travel("Effect");
    //        }
    //    }
    //}
    public void GarbageCollection()
    {
        try
        {
            if (StateMachine.GetCurrentNode() == "End" && Disposable)
            {
                this.QueueFree();
            }
        }
        catch (Exception e)
        {

            throw new Exception(e.Message + this.Name);
        }
       
    }

}


