using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Spell : Node2D
{
	public SpellParams _params;

    
    public Spell()
	{
		_params = new SpellParams();
	}
	
	public List<SpellAction> Actions = new List<SpellAction>();

	public virtual void Cast(SpellParams Params)
	{
		_params = new SpellParams
		{
			ChannelTime = Params.ChannelTime,
			ManaCost = Params.ManaCost,
			Cooldown = Params.Cooldown,
			SpellRange = Params.SpellRange,
			IsReady = Params.IsReady,
			CooldownRemaining = Params.CooldownRemaining,
			Position = Params.Position,
			SpellEffectScene = _params.SpellEffectScene
		};
		
		if (Params.IsReady)
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
					Actions[i].Execute(Params);
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
        _params.IsReady = false;
        _params.CooldownRemaining = _params.Cooldown;
	}

	public override void _Process(double delta)
	{
		if (!_params.IsReady)
		{
            _params.CooldownRemaining -= (float)delta;
			if (_params.CooldownRemaining <= 0)
			{
                _params.CooldownRemaining = 0;
                _params.IsReady = true;
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
