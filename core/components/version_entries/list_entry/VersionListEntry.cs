using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;
using System;

public partial class VersionListEntry : VersionEntryBase
{
    private Label _cSharpLabel;
    private RichTextLabel _titleLabel;
    private Label _pathLabel;

    public override void _Ready()
    {
        base._Ready();
        _cSharpLabel = GetNode<Label>("%CSharpLabel");
        _titleLabel = GetNode<RichTextLabel>("%TitleLabel");
        _pathLabel = GetNode<Label>("%PathLabel");
    }

    public override void SetData(in VersionKey versionKey)
    {
        IsCSharp = versionKey.isDotNet;
        Build = versionKey.buildType;
        _versionStr = versionKey.VersionStr;

        if (IsCSharp)
            _cSharpLabel.Show();
        else
            _cSharpLabel.Hide();

        _titleLabel.Text = $"Version {_versionStr} ".BBCodeColor(ColorTheme.BaseBlue)
            + $"[{versionKey.buildType}]".BBCodeColor(versionKey.buildType.HTMLColor);

        _pathLabel.Text = $"Path: {VersionDatabase.Instance.GetPath(versionKey)}";
    }
}
