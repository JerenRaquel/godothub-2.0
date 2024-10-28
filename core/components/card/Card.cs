using Godot;
using System;

public partial class Card : MarginContainer
{
    [Signal] public delegate void DoubledClickedEventHandler();

    [Export] private Texture2D normalIcon;
    [Export] private Texture2D csharpIcon;

    private Timer _timer;
    private Button _button;
    private TextureRect _icon;
    private Label _version;
    private Label _build;

    private bool _isCSharp = false;
    private string _versionStr;
    private string _buildStr;

    public bool IsCSharp => _isCSharp;
    public string Version => _versionStr;
    public string Build => _buildStr;

    public override void _ExitTree() => _button.Toggled -= OnToggled;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("%Timer");
        _button = GetNode<Button>("%Button");
        _button.Toggled += OnToggled;
        _icon = GetNode<TextureRect>("%Icon");
        _version = GetNode<Label>("%Version");
        _build = GetNode<Label>("%Build");
    }

    public void SetData(string version, string build, bool isCSharp)
    {
        if (isCSharp)
            _icon.Texture = csharpIcon;
        else
            _icon.Texture = normalIcon;

        _version.Text = $"Version {version}";
        _build.Text = build;

        _isCSharp = isCSharp;
        _versionStr = version;
        _buildStr = build;
    }

    private void OnToggled(bool _toggled)
    {
        if (_timer.TimeLeft > 0)
        {
            _timer.Stop();
            EmitSignal(SignalName.DoubledClicked);
            return;
        }
        _timer.Start();
    }
}
