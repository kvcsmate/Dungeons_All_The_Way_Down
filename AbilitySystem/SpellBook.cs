using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Swap(int key, string _spellId)
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
            _node.AddChild(Pages[key].Spell);

        }


    }
}
