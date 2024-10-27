using Godot;
using System;

public partial class Config : InterfaceBase
{
    private CheckButton _fullExecPathCheckButton;
    private CheckButton _absProjPathCheckButton;
    private OptionButton _HUBBehaviorOptionButton;

    public override void _Ready()
    {
        _fullExecPathCheckButton = GetNode<CheckButton>("%FullExecPathCheckButton");
        _absProjPathCheckButton = GetNode<CheckButton>("%AbsProjPathCheckButton");
        _HUBBehaviorOptionButton = GetNode<OptionButton>("%HUBBehaviorOptionButton");
    }

    public override bool GetSettingBool(string settingTag)
    {
        return settingTag switch
        {
            "full_exec_path_check" => _fullExecPathCheckButton.ButtonPressed,
            "abs_proj_path" => _absProjPathCheckButton.ButtonPressed,
            _ => false
        };
    }

    public override int GetSettingOption(string settingTag)
    {
        return settingTag switch
        {
            "HUB_behavior" => _HUBBehaviorOptionButton.Selected,
            _ => 0
        };
    }
}
