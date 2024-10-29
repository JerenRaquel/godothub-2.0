using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Settings : PanelContainer
{
    private VBoxContainer _sectionContainers;

    [Export] public Control[] interfaces;

    private Dictionary<string, Dictionary<string, InterfaceBase>> _interfaces = [];
    private Dictionary<string, TreeSection> _sections = [];

    public override void _ExitTree()
    {
        SaveSettings();

        foreach (Control section in _sectionContainers.GetChildren().Cast<Control>())
            if (section is TreeSection section1)
            {
                section1.InterfaceRequested -= OnInterfaceRequested;
                section1.ToggleOthersOff -= OnToggleOthersOff;
            }
    }

    public override void _Ready()
    {
        _sectionContainers = GetNode<VBoxContainer>("%SectionContainers");

        foreach (Control section in _sectionContainers.GetChildren().Cast<Control>())
            if (section is TreeSection section1)
            {
                section1.InterfaceRequested += OnInterfaceRequested;
                section1.ToggleOthersOff += OnToggleOthersOff;
                _sections.TryAdd(section1.headerName, section1);
                section1.ToggleHeaderOn();
            }

        foreach (Control interfaceControl in interfaces)
        {
            string interfaceGroupName = interfaceControl.Name;
            if (!_interfaces.ContainsKey(interfaceGroupName))
                _interfaces.Add(interfaceGroupName, []);

            foreach (InterfaceBase interfaceChild in interfaceControl.GetChildren().Cast<InterfaceBase>())
            {
                string interfaceName = interfaceChild.Name;
                if (!_interfaces[interfaceGroupName].ContainsKey(interfaceGroupName))
                    _interfaces[interfaceGroupName].Add(interfaceName, interfaceChild);

                interfaceChild.SetAnchorsPreset(LayoutPreset.FullRect);
                interfaceChild.Hide();
                interfaceChild.SettingChanged += (string settingTag) => OnSettingChanged(interfaceGroupName, interfaceName, settingTag);
            }
        }

        _sectionContainers.GetChild<TreeSection>(0).ToggleFirstOn();

        if (SettingsCache.Instance.LoadData())
        {
            foreach (string key in SettingsCache.Instance.Keys)
            {
                SettingsData.ParsedKeyData data = SettingsData.ParseKey(key);
                GetInterface(data.group, data.name)?.SetData(data.tag, SettingsCache.Instance.GetData(key));
            }
        }
    }

    private void SaveSettings()
    {
        foreach (KeyValuePair<string, Dictionary<string, InterfaceBase>> groupEntry in _interfaces)
            foreach (KeyValuePair<string, InterfaceBase> entry in groupEntry.Value)
                foreach (string tag in entry.Value.GetAllSettingTags())
                    OnSettingChanged(groupEntry.Key, entry.Key, tag);
    }

    private InterfaceBase GetInterface(string interfaceGroup, string interfaceName)
    {
        if (!_interfaces.TryGetValue(interfaceGroup, out Dictionary<string, InterfaceBase> interfaceGroupDict)) return null;
        if (!interfaceGroupDict.TryGetValue(interfaceName, out InterfaceBase interfaceControl)) return null;
        return interfaceControl;
    }

    private void OnInterfaceRequested(string interfaceGroup, string interfaceName, bool isOpen)
    {
        Control interfaceChild = GetInterface(interfaceGroup, interfaceName);
        if (interfaceChild == null) return;

        if (isOpen)
            interfaceChild.Show();
        else
            interfaceChild.Hide();
    }

    private void OnToggleOthersOff(string headerName)
    {
        foreach (KeyValuePair<string, Dictionary<string, InterfaceBase>> headers in _interfaces)
        {
            if (headers.Key == headerName) continue;

            _sections[headers.Key].ToggleAllSubButtonsOff();
        }
    }

    private void OnSettingChanged(string group, string section, string settingTag)
    {
        InterfaceBase interfaceControl = GetInterface(group, section);
        SettingsData.Data data = interfaceControl.GetData(settingTag);
        SettingsCache.Instance.AddOrUpdate(
            SettingsData.GenerateKey(group, section, settingTag, data.DataType),
            data
        );
    }

}
