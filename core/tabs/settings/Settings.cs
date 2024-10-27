using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Settings : PanelContainer
{
    private VBoxContainer _sectionContainers;

    [Export] public Control[] interfaces;

    private Dictionary<string, Dictionary<string, Control>> _interfaces = [];

    public override void _ExitTree()
    {
        foreach (TreeSection section in _sectionContainers.GetChildren().Cast<TreeSection>())
            section.InterfaceRequested -= OnInterfaceRequested;
    }

    public override void _Ready()
    {
        _sectionContainers = GetNode<VBoxContainer>("%SectionContainers");

        foreach (TreeSection section in _sectionContainers.GetChildren().Cast<TreeSection>())
            section.InterfaceRequested += OnInterfaceRequested;

        foreach (Control interfaceControl in interfaces)
        {
            string interfaceGroupName = interfaceControl.Name;
            if (!_interfaces.ContainsKey(interfaceGroupName))
                _interfaces.Add(interfaceGroupName, []);

            foreach (Control interfaceChild in interfaceControl.GetChildren().Cast<Control>())
            {
                string interfaceName = interfaceChild.Name;
                if (!_interfaces[interfaceGroupName].ContainsKey(interfaceGroupName))
                    _interfaces[interfaceGroupName].Add(interfaceName, interfaceChild);

                interfaceChild.Hide();
            }
        }

        _sectionContainers.GetChild<TreeSection>(0).ToggleFirstOn();
    }

    public bool GetSettingBool(string interfaceGroup, string interfaceName, string settingTag)
    {
        Control interfaceChild = GetInterface(interfaceGroup, interfaceName);
        return (interfaceChild as IInterfaceBase)?.GetSettingBool(settingTag) ?? false;
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

    private Control GetInterface(string interfaceGroup, string interfaceName)
    {
        if (!_interfaces.TryGetValue(interfaceGroup, out Dictionary<string, Control> interfaceGroupDict)) return null;
        if (!interfaceGroupDict.TryGetValue(interfaceName, out Control interfaceControl)) return null;
        return interfaceControl;
    }

}
