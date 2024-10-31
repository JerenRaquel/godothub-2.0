using Godot;
using System;

public partial class TabBase : PanelContainer
{
    [Signal] public delegate void SettingUpdatedEventHandler(string key);

    public virtual void LoadData() { }
}
