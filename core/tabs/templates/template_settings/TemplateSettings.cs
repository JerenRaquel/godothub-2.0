using Godot;
using System;

public partial class TemplateSettings : MarginContainer
{
    private OptionButton _versionOptionButton;
    private AddDeleteComponent _versionAddDeleteButtons;
    private AddDeleteComponent _tagAddDeleteButtons;

    public override void _Ready()
    {
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
        _versionAddDeleteButtons = GetNode<AddDeleteComponent>("%VersionAddDeleteComponent");
        _tagAddDeleteButtons = GetNode<AddDeleteComponent>("%TagAddDeleteComponent");
    }
}
