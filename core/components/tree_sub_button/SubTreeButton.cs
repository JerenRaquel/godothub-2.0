using Godot;
using System;

public partial class SubTreeButton : MarginContainer
{
    private Button _button;
    private Label _label;

    private Action pressedCallback;

    public string ButtonName => _label.Text;

    public override void _ExitTree()
    {
        _button.Pressed -= pressedCallback;
    }

    public override void _Ready()
    {
        _button = GetNode<Button>("%Button");
        _label = GetNode<Label>("%Label");
    }

    public void Initialize(string name, Action onPressedCallback)
    {
        _label.Text = name;
        pressedCallback = onPressedCallback;
        _button.Pressed += pressedCallback;
    }

    public void ToggleOff()
    {
        _button.SetPressedNoSignal(false);
    }
}
