using Godot;

public partial class FrostNovaBaseAction : SpellAction
{
    [Export] public int Damage = 30;
    [Export] public float FreezeDuration = 2.0f;

    public override void Execute(SpellAttributes spellAttributes)
    {
        if (!spellAttributes.IsReady || spellAttributes.SpellEffectScene == null)
        {
            return;
        }

        var effect = spellAttributes.SpellEffectScene.Instantiate<FrostNovaEffect>();
        GetTree().CurrentScene.AddChild(effect);
        effect.Activate(spellAttributes.Caster, Damage, spellAttributes.SpellRange, FreezeDuration);
    }
}
