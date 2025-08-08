using Godot;
using System;

public partial class SpellParams
{

    [Export] public float ChannelTime = 0;
    [Export] public int ManaCost = 10;
    [Export] public float Cooldown = 0.5f;
    [Export] public float SpellRange;
    public bool IsReady = true;

    public float CooldownRemaining { get; set; }
    [Export] public PackedScene SpellEffectScene;
    
    public Vector2 Position;

    public Player Player;

}
