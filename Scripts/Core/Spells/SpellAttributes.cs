// Shared spell foundation.
using Godot;

public partial class SpellAttributes
{

    [Export] public float ChannelTime = 0;
    [Export] public int ManaCost = 10;
    [Export] public float Cooldown = 0.5f;
    [Export] public float SpellRange;
    [Export] public int Damage = 40;
    [Export] public float Speed = 500f;
    public bool IsReady = true;

    public float CooldownRemaining { get; set; }

    public Vector2 Position;

    public Character Caster;

}
