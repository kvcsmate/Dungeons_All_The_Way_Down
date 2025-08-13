using Godot;
using System.Collections.Generic;
using DungeonsAlltheWayDown.AbilitySystem;
using System;
using System.Text;

public partial class SpellHUD : CanvasLayer
{
    [Export]
    public int NumberOfSlots = 3; // Number of slots in the spell HUD, can be adjusted as needed
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

        for (int i = 0; i < NumberOfSlots; i++)
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

    public void UpdateSpellHUD(int key)
    {
        // Handle the spellbook swap (update UI, etc.)

        var slot = _slots[key];
        if (SpellBook.Pages.ContainsKey(key))
        {
            var spell = SpellBook.Pages[key].Spell;

            slot.Icon.Texture = LoadIcon(SpellBook.Pages[key]);
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
            slot.KeyLabel.Text = GetKeyName(key);
            slot.Icon.Modulate = new Color(1, 1, 1, 0.25f);
            if (slot.CooldownLabel != null)
                slot.CooldownLabel.Visible = false;
            GD.Print("Spell not found for key: " + key);
        }
    }

    private Texture2D LoadIcon(SpellBook.Page page)
    {
        GD.Print("Loading icon for spell: " + page.SpellId);
        if (page == null || page.Spell == null || page.Spell.SpellEffectScene == null)
        {
            return DefaultIcon;
        }
        string iconPath = $"res://Scenes/Nodes/Spells/{page.SpellId}/Icon.png";
        GD.Print("Icon path: " + iconPath);
        var Icon =  GD.Load<Texture2D>(iconPath);

        
        // Fallback if no icon is found
        return Icon ?? DefaultIcon;
    }


}

