using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    [Export]
    public float MovementSpeed = 500.0f;

    [Export]
    public Godot.Collections.Array<PackedScene> Spells;

}

