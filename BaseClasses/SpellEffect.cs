using Godot;
using System;

public abstract partial class SpellEffect : Node2D
{
    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;

    public bool Disposable = true;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        //AnimationTree.AnimationStarted += OnAnimationFinished;
        //get State machine
       StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

        OnInit();      
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
        OnFrame( delta);
        GarbageCollection();
    }

    public void GarbageCollection()
    {
        if (Disposable)
        {
            if (StateMachine.GetCurrentNode() != "Effect")
            {
                StateMachine.Travel("Effect");
            }
        }

        GD.Print(StateMachine.GetCurrentNode());

        if (StateMachine.GetCurrentNode() == "End" && Disposable)
        {
            this.QueueFree();
        }
    }

    public  virtual void OnInit()
    {
        // virtual class, will be filled in inherited members
    }
    public virtual void OnFrame(double delta)
    {
        // virtual class, will be filled in inherited members
    }


}
