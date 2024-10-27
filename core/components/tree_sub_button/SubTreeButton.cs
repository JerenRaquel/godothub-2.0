using Godot;
using System;

public partial class SubTreeButton : MarginContainer
{
    [Signal] public delegate void ToggledEventHandler(string buttonName, bool state);

    private Button _button;
    private Label _label;

    public string ButtonName => _label.Text;
    public bool IsActive => _button.ButtonPressed;

    public override void _ExitTree() => _button.Toggled -= OnButtonToggled;

    public override void _Ready()
    {
        _button = GetNode<Button>("%Button");
        _label = GetNode<Label>("%Label");
    }

    public void Initialize(string name)
    {
        _label.Text = name;
        _button.Toggled += OnButtonToggled;
    }

    public void ToggleOff()
    {
        _button.SetPressedNoSignal(false);
        SetColor(false);
    }

    public void ToggleOn()
    {
        _button.ButtonPressed = true;
        SetColor(true);
    }

    public void KeepOn()
    {
        _button.SetPressedNoSignal(true);
        SetColor(true);
    }

    private void OnButtonToggled(bool toggled)
    {
        if (!toggled)
        {
            _button.SetPressedNoSignal(true);
            return;
        }
        else
            SetColor(true);
        EmitSignal(SignalName.Toggled, _label.Text, toggled);
    }

    private void SetColor(bool on)
    {
        if (on)
            _label.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        else
            _label.AddThemeColorOverride("font_color", new Color("888888"));
    }
}
