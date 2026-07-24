// Shared spell foundation.
using Godot;
using System;

public abstract partial class SpellEffect : CharacterBody2D
{
    [Export]
    public string Id { get; set; }

    [Export]
    public new string Name { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public bool Enabled { get; set; } = true;

    [Export]
    public bool OverrideBaseEffect = false;

    [Signal]
    public delegate void EffectFinishedEventHandler();

    public enum CallerType
    {
        NPC,
        Player
    }

    public Node2D _callerEntity;

    public CallerType Caller;

    public AnimationTree AnimationTree;

    public AnimationNodeStateMachinePlayback StateMachine;

    public bool Disposable = false;

    private bool _finished;

    public virtual void Activate(SpellAttributes spellAttributes)
    {
        GlobalPosition = spellAttributes.Position;

        if (StateMachine != null)
        {
            Disposable = true;
            return;
        }

        FinishEffect();
    }

    protected void FinishEffect()
    {
        if (_finished)
        {
            return;
        }

        _finished = true;
        EmitSignal(SignalName.EffectFinished);
    }

    public virtual void AnimHandler()
    {
        if (Disposable && StateMachine != null)
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
        AnimationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
        if (AnimationTree != null)
        {
            StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        AnimHandler();
        GarbageCollection();
    }

    public void GarbageCollection()
    {
        if (StateMachine == null)
        {
            return;
        }

        if (StateMachine.GetCurrentNode() == "End" && Disposable)
        {
            FinishEffect();
            QueueFree();
        }
    }
}
