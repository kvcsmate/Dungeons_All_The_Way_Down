using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public abstract partial class NPC : LevelEntity
{
    [Export]
    public int Health = 100;

    [Export]
    public bool IsHostile;

    [Export]
    public string Title;

    [Export]
    public string SpriteName;

    public Sprite2D CharacterSprite;

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
                    case StateEnum.Run: StateMachine.Travel("Run"); break;
                    case StateEnum.Death: StateMachine.Travel("Death"); break;
                    default: StateMachine.Travel("Idle"); break;
                }

            }
        }
    }

    public enum StateEnum
    {
        Idle,
        Run,
        Death
    }

    public void DeathCheck()
    {
        if (Health <= 0)
        {
            Disposable = true;
        }
    }

    public override void _Ready()
    {
        AnimationTree = this.GetNode<AnimationTree>("AnimationTree");
        //AnimationTree.AnimationStarted += OnAnimationFinished;
        //get State machine
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

        GD.Print(StateMachine + this.Name);

        LoadSprite(GetSpriteName());

    }

    public string GetSpriteName()
    {
        if (SpriteName is not null)
        {
            return SpriteName;
        }
        else
        {
            foreach (var item in this.GetChildren())
            {
                
                if (item is Sprite2D || item is AnimatedSprite2D)
                {
                    return item.Name;
                }
            }
        }

        return "Sprite2D";
    }

    public abstract void LoadSprite(string SpriteName);
    //{
    //    CharacterSprite = this.GetNode<Sprite2D>(SpriteName);
    //}


    public virtual void OnHit(int Damage)
    {
        Health -= Damage;
        GD.Print(Health);

        DeathCheck();
    }

    public override void AnimHandler()
    {
        if (Disposable)
        {
            CurrentState = StateEnum.Death;
        }
    }
}

