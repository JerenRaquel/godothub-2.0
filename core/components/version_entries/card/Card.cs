using Godot;
using System;

public partial class Card : VersionEntryBase
{
    private Label _cSharpLabel;
    private Label _version;
    private Label _build;

    public override void _Ready()
    {
        base._Ready();
        _cSharpLabel = GetNode<Label>("%CSharpLabel");
        _version = GetNode<Label>("%Version");
        _build = GetNode<Label>("%Build");
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

        _version.Text = $"Version {version}";
        _build.Text = VersionData.BuildEnumToString(build);
        _build.AddThemeColorOverride("font_color", new Color(ColorTheme.GetColorFromBuild(build)));
    }
}
