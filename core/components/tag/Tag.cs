using Godot;
using System;

public partial class Tag : PanelContainer
{
    protected Label _tagLabel;
    private Panel _colorRect;

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
        _colorRect = GetNode<Panel>("%Color");
        _tagLabel = GetNode<Label>("%Label");
    }

    public void SetData(string tag, string colorCode, bool displayed = false)
    {
        _colorRect.SelfModulate = new(colorCode);
        _tagLabel.Text = tag;
        Displayed = displayed;
    }
}
