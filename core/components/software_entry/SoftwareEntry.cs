using Godot;
using System;

public partial class SoftwareEntry : MarginContainer
{
    private Button _favoriteButton;
    private TextureRect _colorTab;
    private Label _nameLabel;
    private Label _pathLabel;
    private Label _commandLabel;

    public DoubleClickButton MainButton { get; private set; }
    public string SoftwareTag { get; private set; }

    public override void _Ready()
    {
        MainButton = GetNode<DoubleClickButton>("%DoubleClickButton");
        _favoriteButton = GetNode<Button>("%FavoriteButton");
        _favoriteButton.Toggled += OnFavoriteToggled;
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
        bool favorited = TagCache.Instance.IsFavorited(name);

        _nameLabel.Text = name;
        SoftwareTag = name;
        _pathLabel.Text = $"└─> Path: {path}";
        _commandLabel.Text = $"       └─> CLI: {command}";
        _colorTab.Modulate = new(colorCode);
        _favoriteButton.SetPressedNoSignal(favorited);
    }

    private void OnFavoriteToggled(bool state) => TagCache.Instance.SetFavorited(SoftwareTag, state);
}
