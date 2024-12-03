using Godot;
using System;

public partial class ProjectPathDefaults : InterfaceBase
{
    public const string RENDERING_DEVICE_TAG = "rendering_device";
    public const string NAMING_SCHEME_TAG = "naming_scheme";
    public const string LAUNCH_BEHAVIOR_TAG = "launch_behavior";

    private OptionButton _renderingDeviceOptionButton;
    private OptionButton _namingSchemeOptionButton;
    private OptionButton _launchBehaviorOptionButton;

    public override void _Ready()
    {
        _renderingDeviceOptionButton = GetNode<OptionButton>("RenderingDeviceOptionButton");
        _namingSchemeOptionButton = GetNode<OptionButton>("NamingSchemeOptionButton");
        _launchBehaviorOptionButton = GetNode<OptionButton>("LaunchBehaviorOptionButton");

        _renderingDeviceOptionButton.ItemSelected += _ => EmitSignal(SignalName.SettingChanged, RENDERING_DEVICE_TAG);
        _namingSchemeOptionButton.ItemSelected += _ => EmitSignal(SignalName.SettingChanged, NAMING_SCHEME_TAG);
        _launchBehaviorOptionButton.ItemSelected += _ => EmitSignal(SignalName.SettingChanged, LAUNCH_BEHAVIOR_TAG);
    }

    public override string[] GetAllSettingTags() => [RENDERING_DEVICE_TAG, NAMING_SCHEME_TAG, LAUNCH_BEHAVIOR_TAG];

    public override SettingsData.Data GetData(string settingTag)
    {
        return settingTag switch
        {
            RENDERING_DEVICE_TAG => _renderingDeviceOptionButton.Selected,
            NAMING_SCHEME_TAG => _namingSchemeOptionButton.Selected,
            LAUNCH_BEHAVIOR_TAG => _launchBehaviorOptionButton.Selected,
            _ => new()
        };
    }

    public override void SetData(string settingTag, SettingsData.Data data)
    {
        switch (settingTag)
        {
            case RENDERING_DEVICE_TAG:
                _renderingDeviceOptionButton.Select(data);
                break;
            case NAMING_SCHEME_TAG:
                _namingSchemeOptionButton.Select(data);
                break;
            case LAUNCH_BEHAVIOR_TAG:
                _launchBehaviorOptionButton.Select(data);
                break;
            default: break;
        }
    }

}
