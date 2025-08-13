using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DungeonsAlltheWayDown.AbilitySystem.SpellBook;

public abstract partial class Spell : Node2D
{
    private float _channelTime = 0;
    private int _manaCost = 10;
    private float _cooldown = 0.5f;
    private float _spellRange;
    private bool _isReady = true;
    private float _cooldownRemaining;
    private PackedScene _spellEffectScene;
    private readonly List<SpellAction> _actions = new List<SpellAction>();
    private float _speed = 500f;

    [Export]
    public float ChannelTime
    {
        get => _channelTime;
        set => _channelTime = value;
    }

    [Export]
    public int ManaCost
    {
        get => _manaCost;
        set => _manaCost = value;
    }

    [Export]
    public float Cooldown
    {
        get => _cooldown;
        set => _cooldown = value;
    }

    [Export]
    public float SpellRange
    {
        get => _spellRange;
        set => _spellRange = value;
    }

    [Export]
    public bool IsReady
    {
        get => _isReady;
        set => _isReady = value;
    }

    public float CooldownRemaining
    {
        get => _cooldownRemaining;
        set => _cooldownRemaining = value;
    }

    [Export]
    public PackedScene SpellEffectScene
    {
        get => _spellEffectScene;
        set => _spellEffectScene = value;
    }

    [Export]
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public List<SpellAction> Actions => _actions;

    public virtual void Cast(SpellParams Params)
    {
        SpellAttributes Attributes = new SpellAttributes
        {
            ChannelTime = _channelTime,
            ManaCost = _manaCost,
            Cooldown = _cooldown,
            SpellRange = _spellRange,
            IsReady = _isReady,
            CooldownRemaining = _cooldownRemaining,
            Position = Params.Position,
            Caster = Params.Caster,
            SpellEffectScene = _spellEffectScene,
            Speed = Speed,
        };

        if (_isReady)
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i].OverrideBaseAction)
                {
                    _actions.First(a => a.Id == "base").Enabled = false;
                }

                if (_actions[i].Enabled)
                {
                    _actions[i].Execute(Attributes);
                }
            }
        }
        StartCooldown();
    }

    public override void _Ready()
    {
        // Initialize the spell (e.g., load resources, set up effects)
        base._Ready();
        LoadActions();
    }

    protected void StartCooldown()
    {
        _isReady = false;
        _cooldownRemaining = _cooldown;
    }

    public override void _Process(double delta)
    {
        if (!_isReady)
        {
            _cooldownRemaining -= (float)delta;
            if (_cooldownRemaining <= 0)
            {
                _cooldownRemaining = 0;
                _isReady = true;
            }
        }
    }

   // public abstract void GetSpellPath();

    public void LoadActions()
    {
        string ActionPath = $"Scenes//Nodes//Spells//{this.GetType().Name}//Actions";
        GD.Print("Loading actions from path: " + ActionPath);
        string[] fileEntries = Directory.GetFiles(ActionPath, "*.tscn").ToArray();
        GD.Print(fileEntries);
        foreach (string fileName in fileEntries)
        {
            if (fileName.EndsWith("Action.tscn"))
            {
                var actionScene = (PackedScene)GD.Load(fileName);
                var action = (SpellAction)actionScene.Instantiate();
                this.AddChild(action);
                _actions.Add(action);
            }
        }
    }
}
