using Godot;
using System;

public partial class DirectionIndicator : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 direction = new Vector2(
			Input.GetJoyAxis(0, JoyAxis.RightX),
			Input.GetJoyAxis(0, JoyAxis.RightY));
		if (direction.Length() ==0)
		{
			direction = new Vector2(
				Input.GetJoyAxis(0, JoyAxis.LeftX),
				Input.GetJoyAxis(0, JoyAxis.LeftY));
			
		}
		Rotation = direction.Angle() + Mathf.Pi / 2f; // Correct for 90 degree offset

	}
}
