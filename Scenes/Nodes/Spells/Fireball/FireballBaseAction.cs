using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class FireballBaseAction : SpellAction
{
    public override void Execute(SpellAttributes spellAttributes)
    {
        if (!spellAttributes.IsReady) return;

        // Spawn fireball effect
        var spellEffect = (Node2D)spellAttributes.SpellEffectScene.Instantiate();
        GetTree().Root.AddChild(spellEffect);
        spellEffect.GlobalPosition = spellAttributes.Position;

    }
}

