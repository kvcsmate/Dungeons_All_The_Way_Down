// Ability-system infrastructure.
using Godot;
using System;
using System.Collections.Generic;

namespace DungeonsAlltheWayDown.AbilitySystem
{
    public partial class SpellBook
    {
        SpellLoader _spellLoader;
        Node _node;
        public SpellBook(SpellLoader spellLoader, Node2D caller)
        {
            _spellLoader = spellLoader;
            _node = caller; 
        }
        public Dictionary<int, Page> Pages = new Dictionary<int, Page>();

        public class Page
        {
            public string SpellId;
            public Spell Spell;
        }

        public void Update(int key, string _spellId)
        {
            try
            {
                // burnt in spell picking, we'll update it later
                if (Pages.ContainsKey(key))
                {
                    Pages[key].Spell.Dispose();
                }

                Pages[key] = new Page
                {
                    SpellId = _spellId,
                    Spell = (Spell)_spellLoader.SpellScenes[_spellId].Instantiate()
                };
                GD.Print("Instantiated spell: " + Pages[key].Spell.Name);
                _node.AddChild(Pages[key].Spell);
            }
            catch (Exception e)
            {
                GD.PrintErr("Error loading spell: " + _spellId + " - " + e.Message);
            }
        }


    }
}
