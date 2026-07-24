// Shared spell foundation.
using Godot;
using System.Collections.Generic;

public abstract partial class Spell : Node2D
{
    private const string SpellEffectsNodeName = "SpellEffects";

    private float _channelTime = 0;

    private int _manaCost = 10;

    private float _cooldown = 0.5f;

    private float _spellRange;

    private bool _isReady = true;

    private float _cooldownRemaining;

    private SpellAttributes _activeSpellAttributes;

    private int _currentEffectIndex = -1;

    private float _speed = 500f;

    private readonly List<SpellEffect> _spellEffectTemplates = new();

    [Export]
    public Texture2D Icon { get; set; }

    private void StartNextEffect()
    {
        if (_activeSpellAttributes == null)
        {
            return;
        }

        _currentEffectIndex++;
        while (_currentEffectIndex < _spellEffectTemplates.Count)
        {
            SpellEffect effectTemplate = _spellEffectTemplates[_currentEffectIndex];
            if (effectTemplate == null)
            {
                _currentEffectIndex++;
                continue;
            }

            SpellEffect effect = effectTemplate.Duplicate() as SpellEffect;
            if (effect == null)
            {
                _currentEffectIndex++;
                continue;
            }

            GetTree().Root.AddChild(effect);
            effect.EffectFinished += () => OnEffectFinished(effect);
            effect.Activate(_activeSpellAttributes);
            return;
        }

        _currentEffectIndex = -1;
        _activeSpellAttributes = null;
    }

    private void OnEffectFinished(SpellEffect effect)
    {
        if (IsInstanceValid(effect) && !effect.IsQueuedForDeletion())
        {
            effect.QueueFree();
        }

        StartNextEffect();
    }

    private void LoadSpellEffectTemplates()
    {
        _spellEffectTemplates.Clear();

        Node effectsRoot = GetNodeOrNull(SpellEffectsNodeName);
        if (effectsRoot == null)
        {
            return;
        }

        foreach (Node child in effectsRoot.GetChildren())
        {
            if (child is not SpellEffect effect)
            {
                continue;
            }

            effectsRoot.RemoveChild(effect);
            _spellEffectTemplates.Add(effect);
        }
    }

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
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

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
            Speed = Speed,
        };

        if (_isReady && _spellEffectTemplates.Count > 0)
        {
            _activeSpellAttributes = Attributes;
            _currentEffectIndex = -1;
            StartNextEffect();
            StartCooldown();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        LoadSpellEffectTemplates();
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

}
