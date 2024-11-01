using Godot;
using System;

[GlobalClass]
public partial class TabBase : PanelContainer
{
    [Signal] public delegate void SettingUpdatedEventHandler(string key);

    public virtual void LoadData() { }
}
