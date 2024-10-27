using Godot;
using System;

public partial class InterfaceBase : HBoxContainer
{
    public virtual bool GetSettingBool(string _settingTag) => false;
    public virtual int GetSettingOption(string _settingTag) => 0;
}
