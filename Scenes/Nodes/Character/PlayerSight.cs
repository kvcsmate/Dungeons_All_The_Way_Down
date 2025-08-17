using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpRequest;

namespace DungeonsAlltheWayDown.Scenes.Nodes.Character
{
    public partial class PlayerSight : Node2D
    {
        public readonly List<Vector2> SightMatrix;

        [Export]
        public bool ShowDebugMarkers = false;

        [Export]
        public int Precision = 180;

        [Export]
        public float Radius = 1000f;

        [Export]
        public float RaycastRadius = 100f;


        private int _precision;
        private float _radius;

        private Vector2 testvector;
        private Vector2 rotationvector;

        private String MarkerLocation = "res://Scenes//Nodes//HUD//Marker.tscn";

        PackedScene MarkerScene;

        private Node2D currentMarker;

        private List<Node2D> testMarkers;

        private CharacterBody2D _player;
        private Rid _playerRID;

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

                SightMatrix.Add(new Vector2(1, 0).Rotated(i * (6.283f / _precision)) * _radius);
            }
        }

        public override void _Ready()
        {
            _player = GetParent() as CharacterBody2D;
            _playerRID = _player.GetRid();
            MarkerScene = (PackedScene)GD.Load(MarkerLocation);

            if (ShowDebugMarkers)
            {
                AddTestMarkers();
            }

        }

        private void AddTestMarkers()
        {
            testMarkers = new List<Node2D>();
            for (int i = 0; i < _precision; i++)
            {
                testMarkers.Add(new Node2D());
                testMarkers[i] = (Node2D)MarkerScene.Instantiate();
                this.AddChild(testMarkers[i]);
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
                    Vector2 direction = (rotationvector - GlobalPosition).Normalized();

                    // First raycast: no offset 
                    var result = spaceState.IntersectRay(
                        new PhysicsRayQueryParameters2D
                        {
                            From = GlobalPosition,
                            To = rotationvector,
                            CollisionMask = 1, // Adjust as needed for your collision layers
                            Exclude = new Godot.Collections.Array<Rid> { _playerRID } // Exclude self from raycast
                        }
                    );

                    Vector2 perpendicular = direction.Orthogonal() * RaycastRadius;
                    
                    if (result.Count ==0)
                    {
                        // second raycast: offset by +perpendicular
                        result = spaceState.IntersectRay(
                            new PhysicsRayQueryParameters2D
                            {
                                From = GlobalPosition + perpendicular,
                                To = rotationvector + perpendicular,
                                CollisionMask = 1, // Adjust as needed for your collision layers
                                Exclude = new Godot.Collections.Array<Rid> { _playerRID } // Exclude self from raycast
                            }
                        );
                    }

                    
                    if (result.Count == 0)
                    {
                        Vector2 reflected = perpendicular - 2 * perpendicular.Dot(rotationvector.Normalized()) * rotationvector.Normalized();
                        // If the first raycast hits nothing, we perform a second raycast offset by -perpendicular
                        // Second raycast: offset by -perpendicular (only if first raycast hits nothing)
                        result = spaceState.IntersectRay(
                            new PhysicsRayQueryParameters2D
                            {
                                From = GlobalPosition + reflected,
                                To = rotationvector + reflected,
                                CollisionMask = 1, // Adjust as needed
                                Exclude = new Godot.Collections.Array<Rid> { _playerRID } // Exclude self from raycast
                            }
                        );
                    }

                    if (result.Count != 0)
                    {
                        Vector2 resultposition = (Vector2)result["position"];
                        Vector2 inverseVector = resultposition.Normalized()* -50;
                        SightMatrix[i] = resultposition + inverseVector ;
                    }
                    else
                    {
                        SightMatrix[i] = rotationvector;
                    }

                    if (ShowDebugMarkers)
                    {
                        testMarkers[i].GlobalPosition = SightMatrix[i] ;
                    }
                }
            }
        }
        
        public Vector2 GetClosestSightPoint(Vector2 position)
        {
            Vector2 closestPoint = SightMatrix[0];
            float closestDistance = position.DistanceTo(closestPoint);

            foreach (var point in SightMatrix)
            {
                float distance = position.DistanceTo(point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }
    }
}
