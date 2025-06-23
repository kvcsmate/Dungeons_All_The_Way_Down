using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonsAlltheWayDown.Scenes.Nodes.Character
{
    public partial class PlayerSight : Node2D
    {
        public List<Vector2> SightMatrix;

        [Export]
        public int Precision = 100;

        [Export]
        public float Radius = 1000f;

        private int _precision;
        private float _radius;

        private Vector2 testvector;
        private Vector2 rotationvector;

        private String MarkerLocation = "res://Scenes//Nodes//HUD//Marker.tscn";

        PackedScene MarkerScene;

        private Node2D currentMarker;

        private List<Node2D> testMarkers;

        public PlayerSight()
        {
            testvector = new Vector2(1, 0);
            rotationvector = new Vector2(1, 0);
            _precision = Precision;
            _radius = Radius;
            //Position = player.Position;
            SightMatrix = new List<Vector2>();



            for (int i = 0; i < _precision; i++)
            {

                SightMatrix.Add(new Vector2(1, 0).Rotated(i * (6.283f / _precision))*_radius);
            }
        }

        public override void _Ready()
        {
            MarkerScene = (PackedScene)GD.Load(MarkerLocation);

            //AddTestMarkers();

        }

        private void AddTestMarkers()
        { 
            testMarkers = new List<Node2D>();
            for (int i = 0; i < _precision; i++)
            {
                testMarkers.Add(new Node2D());
                testMarkers[i] = (Node2D)MarkerScene.Instantiate();
                this.AddChild(testMarkers[i]);
                GD.Print("Testmarker added:" + i);
            }
        }
        public override void _PhysicsProcess(double delta)
        {

            if (Engine.GetPhysicsFrames() % 10 == 0)
            {
                var spaceState = GetWorld2D().DirectSpaceState;

                for (int i = 0; i < _precision; i++)
                {

                    rotationvector = GlobalPosition + new Vector2(1, 0).Rotated(i * (6.283f / _precision)) * _radius;
                    //GD.Print(rotationvector.Angle()*180/3.14);
                    var result = spaceState.IntersectRay(
                        new PhysicsRayQueryParameters2D
                        {
                            From = GlobalPosition,
                            To = rotationvector,
                            CollisionMask = 1 // Adjust as needed for your collision layers
                        }
                    );

                    // If nothing blocks the ray or the first hit is the player, player is in sight
                    if (result.Count == 0)
                    {
                        SightMatrix[i] = rotationvector;
                    }
                    else
                    {
                        SightMatrix[i] = (Vector2)result["position"];
                    }
                    //testMarkers[i].GlobalPosition = SightMatrix[i];
                }
            }
        }
    }
}
