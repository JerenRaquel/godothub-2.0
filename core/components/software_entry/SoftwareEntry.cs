using Godot;
using System;

public partial class SoftwareEntry : MarginContainer
{
    private TextureRect _colorTab;
    private Label _nameLabel;
    private Label _pathLabel;
    private Label _commandLabel;

    public DoubleClickButton MainButton { get; private set; }
    public string SoftwareTag { get; private set; }

    public override void _Ready()
    {
        MainButton = GetNode<DoubleClickButton>("%DoubleClickButton");
        _colorTab = GetNode<TextureRect>("%ColorTab");
        _nameLabel = GetNode<Label>("%NameLabel");
        _pathLabel = GetNode<Label>("%PathLabel");
        _commandLabel = GetNode<Label>("%CommandLabel");
    }

    public void SetData(string name)
    {
        string path = TagCache.Instance.GetPath(name);
        string command = TagCache.Instance.GetRAWCommand(name, false);
        string colorCode = TagCache.Instance.GetColor(true, name);

        _nameLabel.Text = name;
        SoftwareTag = name;
        _pathLabel.Text = $"├─> Path: {path}";
        _commandLabel.Text = $"└─> {command}";
        _colorTab.Modulate = new(colorCode);
    }
}
