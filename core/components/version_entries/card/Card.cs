using DataContainer.DatabaseSys.Databases.VersionDatabase;
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

    public override void SetData(in VersionKey versionKey)
    {
        IsCSharp = versionKey.isDotNet;
        Build = versionKey.buildType;
        _versionStr = versionKey.VersionStr;

        if (IsCSharp)
            _cSharpLabel.Show();
        else
            _cSharpLabel.Hide();

        _version.Text = $"Version {_versionStr}";
        _build.Text = versionKey.BuildTypeStr;
        _build.AddThemeColorOverride("font_color", new Color(ColorTheme.GetColorFromBuild(Build)));
    }
}
