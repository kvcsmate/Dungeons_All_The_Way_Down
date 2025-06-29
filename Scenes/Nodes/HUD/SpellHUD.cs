using Godot;
using System.Collections.Generic;
using DungeonsAlltheWayDown.AbilitySystem;

public partial class SpellHUD : CanvasLayer
{
    private class Slot
    {
        public Sprite2D Icon;
        public Label KeyLabel;
        public Label CooldownLabel;
    }

    private List<Slot> _slots = new();

    [Export] public Texture2D DefaultIcon = GD.Load<Texture2D>("res://Game Assets//HUD//Spell_icons//defaultIcon.png");

    public SpellBook SpellBook { get; set; }
    public override void _Ready()
    {
        if (DefaultIcon == null)
        {
            DefaultIcon = GD.Load<Texture2D>("res://Game Assets//HUD//Spell_icons//defaultIcon.png");
        }

        for (int i = 0; i < 4; i++)
        {
            var slotNode = GetNode<Node2D>($"Control/Slot{i}");
            var icon = slotNode.GetNode<Sprite2D>("Icon");
            var label = slotNode.GetNode<Label>("KeyLabel");
            var cdLabel = slotNode.GetNodeOrNull<Label>("CooldownLabel");
            _slots.Add(new Slot { Icon = icon, KeyLabel = label, CooldownLabel = cdLabel });
        }
    }

    public override void _Process(double delta)
    {
        if (SpellBook == null) return;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (SpellBook.Pages.ContainsKey(i))
            {
                var spell = SpellBook.Pages[i].Spell;
                if (slot.Icon.Texture == null)
                    slot.Icon.Texture = DefaultIcon;
                slot.KeyLabel.Text = GetKeyName(i);
                slot.Icon.Modulate = spell.IsReady ? Colors.White : new Color(1,1,1,0.5f);

                if (slot.CooldownLabel != null)
                {
                    if (spell.IsReady)
                    {
                        slot.CooldownLabel.Visible = false;
                    }
                    else
                    {
                        slot.CooldownLabel.Visible = true;
                        slot.CooldownLabel.Text = Mathf.Ceil(spell.CooldownRemaining).ToString();
                    }
                }
            }
            else
            {
                slot.Icon.Texture = DefaultIcon;
                slot.KeyLabel.Text = GetKeyName(i);
                slot.Icon.Modulate = new Color(1,1,1,0.25f);
                if (slot.CooldownLabel != null)
                    slot.CooldownLabel.Visible = false;
            }
        }
    }

    private string GetKeyName(int index)
    {
        return index switch
        {
            0 => "Q",
            1 => "W",
            2 => "E",
            3 => "R",
            _ => string.Empty
        };
    }
}
