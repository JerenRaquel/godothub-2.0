using DataContainer.DatabaseSys.Databases.SettingDatabase;
using Godot;

[GlobalClass]
public partial class InterfaceBase : HBoxContainer
{
    [Signal] public delegate void SettingChangedEventHandler(string settingTag);

    protected VBoxContainer _contentContainer;

    public override void _Ready() => _contentContainer = GetNode<VBoxContainer>("%ContentPanel");

    public virtual string[] GetAllSettingTags() => [];
    public virtual SettingsData GetData(string settingTag) => null;
    public virtual void SetData(SettingsTag tagKey) { }
}
