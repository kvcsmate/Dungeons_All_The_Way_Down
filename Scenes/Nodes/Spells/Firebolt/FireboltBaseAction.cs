using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonsAlltheWayDown.Scenes.Nodes.Spells.Firebolt
{
    public partial class FireboltBaseAction: SpellAction
    {
        public override void Execute(SpellAttributes spellAttributes)
        {
            if (!spellAttributes.IsReady) return;
            Vector2 direction = (spellAttributes.Position - spellAttributes.Caster.GlobalPosition).Normalized();

            // Spawn Fire Bolt effect
            var fireBoltEffect = (Node2D)spellAttributes.SpellEffectScene.Instantiate();
            fireBoltEffect.GlobalPosition = spellAttributes.Caster.GlobalPosition + direction * 150; // Offset to spawn in front of the player
            GetTree().Root.AddChild(fireBoltEffect);

            // Set the direction of the Fire Bolt
            var fireBoltScript = fireBoltEffect as FireboltEffect;
            if (fireBoltScript != null)
            {
                fireBoltScript.Direction = direction;
                fireBoltScript.Damage = spellAttributes.Damage;
                fireBoltScript.Speed = spellAttributes.Speed;
                fireBoltScript.MaxDistance = spellAttributes.SpellRange;
                fireBoltScript.Caster = spellAttributes.Caster;
            }
        }
    }
}
