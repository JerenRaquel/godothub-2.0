using Godot;
using System;

public partial class Tag : PanelContainer
{
    private ColorRect _colorRect;
    private Label _tagLabel;

    public bool Displayed
    {
        get => _tagLabel.Visible;
        set
        {
            if (value) _tagLabel.Show();
            else _tagLabel.Hide();
        }
    }

    public override void _Ready()
    {
        _colorRect = GetNode<ColorRect>("%ColorRect");
        _tagLabel = GetNode<Label>("%Label");
    }

    public void SetData(string tag, string colorCode, bool displayed = false)
    {
        _colorRect.Color = new(colorCode);
        _tagLabel.Text = tag;
        Displayed = displayed;
    }
}
