using Godot;
using System;

public partial class ClickableTag : Tag
{
    [Signal] public delegate void ToggledEventHandler();

    public bool IsSoftware { get; set; } = false;
    public bool Selected
    {
        get => _selectionButton.ButtonPressed;
        set => _selectionButton.SetPressedNoSignal(value);
    }
    public string Text
    {
        get => _tagLabel.Text;
        set => _tagLabel.Text = value;
    }

    private Button _selectionButton;

    public override void _Ready()
    {
        _selectionButton = GetNode<Button>("%Button");
        _selectionButton.Toggled += _ => EmitSignal(SignalName.Toggled);
        base._Ready();
    }

}
