using Godot;
using Godot.Collections;
using System;

public partial class Testenemy_1Script : HostileNPC
{
    
    public new AnimatedSprite2D CharacterSprite;

    [Export]
    public PackedScene FireballScene; // Drag your Fireball.tscn into this in the Inspector

    private float _shootCooldown = 1f; // 5 seconds cooldown
    private float _shootTimer = 0f;

    public override void LoadSprite(string SpriteName)
    {
        CharacterSprite = this.GetNode<AnimatedSprite2D>(SpriteName);
    }

    public Vector2 MovementTarget
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        _shootTimer -= (float)delta; // Countdown timer
        bool playerInSight = CheckForPlayerInSight();

        if (onAlert)
        {
            MoveToRangeOrLastKnownPosition();
        }

        if (playerInSight)
        {
            StopMovement();
            TryShootFireball();
        }
        else
        {
            if (!onAlert)
                StopMovement();
        }
    }

    private void TryShootFireball()
    {
        if (_shootTimer <= 0)
        {
            ShootFireball();
            _shootTimer = _shootCooldown; // reset cooldown
        }
    }

    private void ShootFireball()
    {
        if (FireballScene == null || _target == null)
            return;

        SpellEffect fireball = FireballScene.Instantiate<SpellEffect>();
        fireball.Caller = SpellEffect.CallerType.NPC;
        GetParent().AddChild(fireball);

        // Calculate direction toward player
        Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();

        // Position fireball at enemy's position
        fireball.GlobalPosition = GlobalPosition + direction*150;


        // Spawn Fire Bolt effect
        //fireball.GlobalPosition = _target.GlobalPosition + direction * 150; // Offset to spawn in front of the player

        // Set the direction of the Fire Bolt
        var fireBoltScript = fireball as FireboltEffect;
        if (fireBoltScript != null)
        {
            fireBoltScript.Direction = direction;
            fireBoltScript.Damage = 30;
            fireBoltScript.Speed = 2000;
            fireBoltScript.MaxDistance = 2000;
            fireBoltScript.Caster = this;
        }


        // If your Fireball script has a velocity or direction property, set it:
        if (fireball.HasMethod("SetDirection"))
        {
            fireball.Call("SetDirection", direction);
        }
    }
}
