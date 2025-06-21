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

        private int _precision;
        private float _radius;

        private Vector2 testvector;
        private Vector2 rotationvector;

        private String MarkerLocation = "res://Scenes//Nodes//HUD//Marker.tscn";

        PackedScene MarkerScene;

        private Node2D currentMarker;

        private List<Node2D> testMarkers;

        private Player _player;

        public PlayerSight(int precision,float radius,Player player)
        {
            testvector = new Vector2(1, 0) * 100;
            rotationvector = new Vector2(1, 0) * 100;
            _precision = precision;
            _radius = radius;
            _player = player;
            SightMatrix = new List<Vector2>();
            testMarkers = new List<Node2D>();

            for (int i = 0; i < precision; i++)
            {

                SightMatrix.Add(new Vector2(1, 0).Rotated(i * (6.283f / _precision))*_radius);
            }
        }

        public override void _Ready()
        {
            rotationvector = new Vector2(1, 0).Rotated(0 * (6.283f / _precision)) * _radius;
            MarkerScene = (PackedScene)GD.Load(MarkerLocation);
            //if (currentMarker != null)
            //{
            //    currentMarker.QueueFree();
            //}

            for (int i = 0; i < 2 ; i++) //_precision
            {
                testMarkers.Add(new Node2D());
                testMarkers[i] = (Node2D)MarkerScene.Instantiate();
                this.AddChild(testMarkers[i]);
                GD.Print("Testmarker added:" + i);
            }

            //currentMarker = (Node2D)MarkerScene.Instantiate();
            //this.AddChild(currentMarker);

        }
        public override void _PhysicsProcess(double delta)
        {

            if (Engine.GetPhysicsFrames() % 10 == 0)
            {
                var spaceState = GetWorld2D().DirectSpaceState;

                //for (int i = 0; i < _precision; i++)
                //{

                //    rotationvector = new Vector2(1, 0).Rotated(i * (6.283f / _precision)) * _radius;
                //    //GD.Print(rotationvector.Angle()*180/3.14);
                //    var result = spaceState.IntersectRay(
                //        new PhysicsRayQueryParameters2D
                //        {
                //            From = Position,
                //            To = rotationvector,
                //            CollisionMask = 1 // Adjust as needed for your collision layers
                //        }
                //    );

                //    // If nothing blocks the ray or the first hit is the player, player is in sight
                //    if (result.Count == 0)
                //    {
                //        SightMatrix[i] = rotationvector;
                //    }
                //    else
                //    {
                //        GD.Print("collision");
                //        SightMatrix[i] = ((Vector2)result["position"]);

                //    }
                //    //GD.Print(SightMatrix[i]);
                //    testMarkers[i].Position = SightMatrix[i];

                //}

                //rotationvector = new Vector2(1, 0).Rotated(0 * (6.283f / _precision)) * _radius;
                
                var exclude = new Array<Rid> { _player.GetRid() }; // Exclude self from raycast
                //GD.Print(rotationvector.Angle()*180/3.14);

                var result = spaceState.IntersectRay(
                    new PhysicsRayQueryParameters2D
                    {
                        From = Position,
                        To = rotationvector,
                        Exclude = exclude,
                        CollisionMask = 1 // Adjust as needed for your collision layers
                    }
                );

                // If nothing blocks the ray or the first hit is the player, player is in sight
                GD.Print(result.Count);
                if (result.Count == 0)
                {
                    testvector = rotationvector;
                }
                else
                {
                    GD.Print(result["collider"]);
                    testvector = ((Vector2)result["position"]);

                }
                //GD.Print(SightMatrix[i]);
                testMarkers[0].Position = rotationvector;
                testMarkers[1].Position = Position;

            }


        }
    }
}
