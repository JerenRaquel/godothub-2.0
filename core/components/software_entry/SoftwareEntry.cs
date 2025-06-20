using DataContainer.DatabaseSys.Databases.TagDatabase;
using Godot;
using System;

public partial class SoftwareEntry : MarginContainer
{
    [Signal] public delegate void FavoriteToggledEventHandler();

    private Button _favoriteButton;
    private TextureRect _colorTab;
    private Label _nameLabel;
    private Label _pathLabel;
    private Label _commandLabel;

    public DoubleClickButton MainButton { get; private set; }
    public TagKey SoftwareTagKey { get; private set; }

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

    public void SetData(in TagKey tagKey)
    {
        TagCommand commandData = TagDatabase.Instance.GetCommandData(tagKey);
        string path = commandData.Path;
        string command = commandData.PrettyCommand;
        string htmlColor = TagDatabase.Instance.GetHTMLColor(tagKey);
        bool IsFavorited = TagDatabase.Instance.IsFavorited(tagKey);

        SoftwareTagKey = tagKey;
        _nameLabel.Text = tagKey.TagName;
        _pathLabel.Text = $"└─> Path: {path}";
        _commandLabel.Text = $"       └─> CLI: {command}";
        _colorTab.Modulate = new(htmlColor);
        _favoriteButton.SetPressedNoSignal(IsFavorited);
    }

    private void OnFavoriteToggled(bool state)
    {
        TagDatabase.Instance.SetFavorited(SoftwareTagKey, state);
        EmitSignal(SignalName.FavoriteToggled);
    }
}
