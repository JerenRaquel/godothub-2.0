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

    public override void SetData(string version, VersionData.BuildType build, bool isCSharp)
    {
        IsCSharp = isCSharp;
        Build = build;
        _versionStr = version;

        if (isCSharp)
            _cSharpLabel.Show();
        else
            _cSharpLabel.Hide();

        _titleLabel.Text = $"Version {version} ".BBCodeColor(ColorTheme.BaseBlue)
            + $"[{VersionData.BuildEnumToString(build)}]".BBCodeColor(ColorTheme.GetColorFromBuild(build));

        _pathLabel.Text = $"Path: {VersionCache.Instance.GetPath(VersionData.GenerateKey(version, isCSharp, build))}";
    }
}
