using System;
using Godot;

[GlobalClass]
public partial class InterfaceBase : HBoxContainer
{
    [Signal] public delegate void SettingChangedEventHandler(string settingTag);

    protected VBoxContainer _contentContainer;

    public override void _Ready() => _contentContainer = GetNode<VBoxContainer>("%ContentPanel");

    public virtual string[] GetAllSettingTags() => [];
    public virtual SettingsData.Data GetData(string settingTag) => new();
    public virtual void SetData(string settingTag, SettingsData.Data data) { }
}
