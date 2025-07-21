using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public abstract partial class Spell : Node2D
{
	

	protected SpellAttributes Attributes { get; set; }
	public Spell()
	{
		Attributes = new SpellAttributes();
	}
	
	public List<SpellAction> Actions = new List<SpellAction>();

	public class SpellParams
	{
		public Vector2 Position;
		public Player Player;
	}
	public void Cast(SpellParams p)
	{
		if (Attributes.IsReady)
		{
			/* because most of the Actions are modifying variables,
			   it is really wasteful to call the whole list,
			   effectively overwriting the same values every time.
			   Since we've no idea how the actions will look like in the future,
			   let's keep it this way for now. One way would be, to set the Enabled
			   to false when it's done with its job. */
			for (int i = 0; i < Actions.Count; i++)
			{

				if (Actions[i].OverrideBaseAction)
				{
					Actions.First(a => a.Id == "base").Enabled = false;
				}

				if (Actions[i].Enabled)
				{
					Actions[i].Execute(Attributes);
				}
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Initialize the spell (e.g., load resources, set up effects)
	}

	protected void StartCooldown()
	{
		Attributes.IsReady = false;
		Attributes.CooldownRemaining = Attributes.Cooldown;
	}

	public override void _Process(double delta)
	{
		if (!Attributes.IsReady)
		{
			Attributes.CooldownRemaining -= (float)delta;
			if (Attributes.CooldownRemaining <= 0)
			{
				Attributes.CooldownRemaining = 0;
				Attributes.IsReady = true;
			}
		}
	}
	public void LoadActions()
	{ 
		            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(this.GetPath(), "*.tscn").ToArray();
            foreach (string fileName in fileEntries)
            {
				var actionScene = (PackedScene)GD.Load(fileName);
				var action = (SpellAction)actionScene.Instantiate();
				this.AddChild(action);
               	Actions.Add(action);
            }   
	}
	
}
