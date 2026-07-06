using Godot;
using System.Collections.Generic;
using Dungeons_All_The_Way_Down.BaseClasses.LevelEntities;

public partial class FrostNovaEffect : Node2D
{
    private const float AnimationDuration = 0.45f;
    private float _radius;
    private float _elapsed;

    public void Activate(Character caster, int damage, float radius, float freezeDuration)
    {
        _radius = radius;
        GlobalPosition = caster.GlobalPosition;
        QueueRedraw();

        var shape = new CircleShape2D { Radius = radius };
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = shape,
            Transform = new Transform2D(0.0f, GlobalPosition),
            CollisionMask = uint.MaxValue,
            CollideWithAreas = false,
            CollideWithBodies = true,
            Exclude = new Godot.Collections.Array<Rid> { caster.GetRid() }
        };

        var hitCharacters = new HashSet<Character>();
        foreach (var result in GetWorld2D().DirectSpaceState.IntersectShape(query, 64))
        {
            if (result["collider"].AsGodotObject() is not Character target || !hitCharacters.Add(target))
            {
                continue;
            }

            if (target.HasMethod("OnHit"))
            {
                target.Call("OnHit", damage);
            }
            target.ApplyStatusEffect(freezeDuration, StatusEffect.Freeze);
        }
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;
        QueueRedraw();
        if (_elapsed >= AnimationDuration)
        {
            QueueFree();
        }
    }

    public override void _Draw()
    {
        float progress = Mathf.Clamp(_elapsed / AnimationDuration, 0.0f, 1.0f);
        float currentRadius = Mathf.Lerp(_radius * 0.15f, _radius, Mathf.Ease(progress, -2.0f));
        float alpha = 1.0f - progress;

        DrawCircle(Vector2.Zero, currentRadius, new Color(0.25f, 0.75f, 1.0f, alpha * 0.16f));
        DrawArc(Vector2.Zero, currentRadius, 0.0f, Mathf.Tau, 96,
            new Color(0.75f, 0.95f, 1.0f, alpha), 12.0f, true);
        DrawArc(Vector2.Zero, currentRadius * 0.72f, 0.0f, Mathf.Tau, 72,
            new Color(0.35f, 0.8f, 1.0f, alpha * 0.8f), 5.0f, true);
    }
}
