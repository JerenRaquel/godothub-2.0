using Godot;

public partial class Notification : HBoxContainer
{
    private Label _label;
    private Button _deleteButton;
    private Timer _timer;
    private ProgressBar _progressBar;

    private bool _isReadied = false;

    public override void _Ready()
    {
        _label = GetNode<Label>("%Label");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += OnDelete;
        _deleteButton.MouseEntered += OnMouseEntered;
        _deleteButton.MouseExited += OnMouseExited;
        _timer = GetNode<Timer>("%Timer");
        _timer.Timeout += OnDelete;
        _progressBar = GetNode<ProgressBar>("%ProgressBar");
        _progressBar.Value = 100;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Start(string message, string colorCode, double delay)
    {
        if (delay <= 0)
        {
            OnDelete();
            return;
        }

        _label.AddThemeColorOverride("font_color", new Color(colorCode));
        _timer.WaitTime = delay;
        _label.Text = message;
        _isReadied = true;
        _timer.Start();
    }

    public override void _PhysicsProcess(double delta)
        => _progressBar.Value = _timer.TimeLeft / _timer.WaitTime * 100.0;

    private void OnDelete() => QueueFree();

    private void OnMouseEntered()
    {
        if (!_isReadied) return;

        SetPhysicsProcess(false);
        _progressBar.Value = 100;
        _timer.Stop();
    }

    private void OnMouseExited()
    {
        if (!_isReadied) return;

        _timer.Start();
        SetPhysicsProcess(true);
    }
}
