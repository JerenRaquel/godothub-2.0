using System;
using Godot;

[GlobalClass]
public partial class InterfaceBase : HBoxContainer
{
    public enum SettingType { BOOL = 0, OPTION = 1, LIST = 2 }

    [Signal] public delegate void SettingChangedEventHandler(string settingTag, int type);

    private string _group;
    private string _section;
    private Action<string, string, string, int> _callback;

    protected VBoxContainer _contentContainer;

    public override void _ExitTree() => SettingChanged -= OnSettingChanged;

    public override void _Ready() => _contentContainer = GetNode<VBoxContainer>("%ContentPanel");

    public void Initialize(string group, string section, Action<string, string, string, int> callback)
    {
        _group = group;
        _section = section;
        _callback = callback;
        SettingChanged += OnSettingChanged;
    }

    public virtual string[] GetAllSettingTags() => [];
    public virtual bool GetSettingBool(string _settingTag) => false;
    public virtual int GetSettingOption(string _settingTag) => 0;
    public virtual string[] GetSettingTagEntries(string _settingTag) => [];

    private void OnSettingChanged(string settingTag, int type) => _callback.Invoke(_group, _section, settingTag, type);
}
