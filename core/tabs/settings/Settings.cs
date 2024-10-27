using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Settings : PanelContainer
{
    [Signal] public delegate void SettingChangedEventHandler(string group, string section, string settingTag, int type);

    private VBoxContainer _sectionContainers;

    [Export] public Control[] interfaces;

    private Dictionary<string, Dictionary<string, InterfaceBase>> _interfaces = [];
    private Dictionary<string, TreeSection> _sections = [];

    public override void _ExitTree()
    {
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
                interfaceChild.Initialize(interfaceGroupName, interfaceName, OnSettingChanged);
            }
        }

        _sectionContainers.GetChild<TreeSection>(0).ToggleFirstOn();
    }

    public bool GetSettingBool(string interfaceGroup, string interfaceName, string settingTag)
    {
        return GetInterface(interfaceGroup, interfaceName)?.GetSettingBool(settingTag) ?? false;
    }

    public int GetSettingOption(string interfaceGroup, string interfaceName, string settingTag)
    {
        return GetInterface(interfaceGroup, interfaceName)?.GetSettingOption(settingTag) ?? 0;
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

    private void OnSettingChanged(string group, string section, string settingTag, int type)
    {
        EmitSignal(SignalName.SettingChanged, group, section, settingTag, type);
    }

}
