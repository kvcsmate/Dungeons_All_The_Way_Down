using Godot;

public partial class EnemyHealthBar : Node2D
{
    private ProgressBar _bar;

    public override void _Ready()
    {
        _bar = GetNode<ProgressBar>("ProgressBar");
    }

    public void UpdateHealth(int health, int maxHealth)
    {
        if (_bar != null)
        {
            _bar.MaxValue = maxHealth;
            _bar.Value = health;
        }
    }
}
