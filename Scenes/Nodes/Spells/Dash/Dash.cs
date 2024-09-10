using Godot;
using System;

public partial class Dash : Spell
{
    [Export] public float DashDistance = 600f;
    [Export] public float DashSpeed = 5000f;
    private Vector2 _dashDirection;
    private float _dashDistance;
    private bool _isDashing = false;
    Player _player;

    //[Signal]
   // public delegate void DashFinishedEventHandler();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _player = this.GetParent<Player>();
    }

    public override void Cast(SpellParams p)
    {
        _dashDirection = (p.Position - ((CharacterBody2D)GetParent()).GlobalPosition).Normalized();
        _isDashing = true;
        //_player.StopMovement();
        _dashDistance = DashDistance;
        _player.IsDisplaced = true;



        StartCooldown();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDashing)
        {
            Vector2 movement = _dashDirection * DashSpeed * (float)delta;
            float remainingDistance = _dashDistance - _player.GlobalPosition.DistanceTo(_player.GlobalPosition + movement);

            GD.Print("parent global pos:" + _player.GlobalPosition);
            GD.Print("movement:" + movement);
            if (remainingDistance <= 0)
            {

                _isDashing = false;

                _player.Velocity = Vector2.Zero;// _dashDirection * remainingDistance;
                _player.MoveAndSlide(); // Finish the dash movement 

                _player.IsDisplaced = false;
                
             //   EmitSignal(SignalName.DashFinished);
            }
            else
            {
                if (Engine.GetPhysicsFrames() % 3 == 0)
                {
                    var spellEffect = (Node2D)SpellEffectScene.Instantiate();
                    GetTree().Root.AddChild(spellEffect);
                    spellEffect.GlobalPosition = _player.Position;
                }
                _player.Velocity = movement;
                _player.MoveAndSlide();
                _dashDistance -= movement.Length();
            }
        }
    }

}
