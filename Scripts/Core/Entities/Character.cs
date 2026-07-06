// Shared entity foundation.
using Dungeons_All_The_Way_Down.BaseClasses.LevelEntities;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract partial class Character : LevelEntity
{
    public abstract override void AnimHandler();

    public abstract void StopMovement();

    public bool IsDisplaced = false;

    [Export]
    public int MaxHealth = 100;

    [Export]
    public int Health = 100;

    [Export]
    public string Title;

    [Export]
    public string SpriteName;

    private int _activeSlowCount;
    private float _movementSpeedBeforeSlow;
    private int _activeFreezeCount;
    private float _movementSpeedBeforeFreeze;

    [Export]
    public float MovementSpeed = 500.0f;

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float SlowSpeedMultiplier = 0.5f;

    [Export]
    public Godot.Collections.Array<PackedScene> Spells;

    private readonly List<Tuple<float, StatusEffect>> _statusEffects = new();

    public IReadOnlyList<Tuple<float, StatusEffect>> StatusEffects => _statusEffects;

    public event Action<StatusEffect, bool> StatusEffectChanged;

    protected Character()
    {
        StatusEffectChanged += HandleStatusEffectChanged;
    }

    // Connect a spell's (timer, statusType) signal to this method.
    public void ApplyStatusEffect(float timer, StatusEffect statusType)
    {
        if (timer <= 0)
        {
            return;
        }

        var activeEffect = new Tuple<float, StatusEffect>(timer, statusType);
        _statusEffects.Add(activeEffect);
        StatusEffectChanged?.Invoke(statusType, true);
        _ = ExpireStatusEffect(activeEffect);
    }

    private async Task ExpireStatusEffect(Tuple<float, StatusEffect> activeEffect)
    {
        SceneTreeTimer timer = GetTree().CreateTimer(activeEffect.Item1);
        await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

        if (_statusEffects.Remove(activeEffect))
        {
            StatusEffectChanged?.Invoke(activeEffect.Item2, false);
        }
    }

    private void HandleStatusEffectChanged(StatusEffect statusType, bool applied)
    {
        if (statusType == StatusEffect.Freeze)
        {
            if (applied)
            {
                if (_activeFreezeCount == 0)
                {
                    _movementSpeedBeforeFreeze = MovementSpeed;
                    MovementSpeed = 0.0f;
                    StopMovement();
                }

                _activeFreezeCount++;
            }
            else
            {
                _activeFreezeCount--;
                if (_activeFreezeCount == 0 && !Disposable)
                {
                    MovementSpeed = _movementSpeedBeforeFreeze;
                }
            }
            return;
        }

        if (statusType != StatusEffect.Slow)
        {
            return;
        }

        if (applied)
        {
            if (_activeSlowCount == 0)
            {
                _movementSpeedBeforeSlow = MovementSpeed;
                MovementSpeed *= Mathf.Clamp(SlowSpeedMultiplier, 0.0f, 1.0f);
            }

            _activeSlowCount++;
            return;
        }

        _activeSlowCount--;
        if (_activeSlowCount == 0)
        {
            MovementSpeed = _movementSpeedBeforeSlow;
        }
    }
}
