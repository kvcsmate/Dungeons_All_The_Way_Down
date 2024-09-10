using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonsAlltheWayDown.AbilitySystem
{
    public partial class AbilityInputMap 
    {
        SpellBook _spellBook;
        public AbilityInputMap(SpellBook spellBook)
        {
            _spellBook = spellBook;
        }
        public void HandleInput(InputEvent @event, Spell.SpellParams spellp)
        {
            int Spellindex= -1 ;
            Spellindex = @event.IsActionPressed("Ability0") ? 0 : Spellindex;
            Spellindex = @event.IsActionPressed("Ability1") ? 1 : Spellindex;
            Spellindex = @event.IsActionPressed("Ability2") ? 2 : Spellindex;
            Spellindex = @event.IsActionPressed("Ability3") ? 3 : Spellindex;

            if (Spellindex >= 0)
            {

                _spellBook.Pages[Spellindex].Spell.Cast(spellp);
            }

        }
    }
}
