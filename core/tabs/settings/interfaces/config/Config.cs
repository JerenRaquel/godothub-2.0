using Godot;
using System;

public partial class Config : HBoxContainer, IInterfaceBase
{
    private CheckButton _fullExecPathCheckButton;
    private CheckButton _absProjPathCheckButton;
    private CheckButton _closeOnOpenCheckButton;

    public override void _Ready()
    {
        _fullExecPathCheckButton = GetNode<CheckButton>("%FullExecPathCheckButton");
        _absProjPathCheckButton = GetNode<CheckButton>("%AbsProjPathCheckButton");
        _closeOnOpenCheckButton = GetNode<CheckButton>("%CloseOnOpenCheckButton");
    }

    public bool GetSettingBool(string settingTag)
    {
        return settingTag switch
        {
            "full_exec_path_check" => _fullExecPathCheckButton.ButtonPressed,
            "abs_proj_path" => _absProjPathCheckButton.ButtonPressed,
            "close_on_open" => _closeOnOpenCheckButton.ButtonPressed,
            _ => false
        };
    }
}
