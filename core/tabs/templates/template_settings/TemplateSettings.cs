using Godot;
using System;

public partial class TemplateSettings : MarginContainer
{
    private OptionButton _versionOptionButton;

    public override void _Ready()
    {
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
    }
}
