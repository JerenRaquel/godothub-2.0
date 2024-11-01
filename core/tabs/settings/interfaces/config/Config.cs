using Godot;
using System;

public partial class Config : InterfaceBase
{
    public const string ABS_PROJ_PATH_TAG = "abs_proj_path";
    public const string FULL_EXEC_PATH_TAG = "full_exec_path";
    public const string HUB_BEHAVIOR_TAG = "HUB_behavior";

    private CheckButton _fullExecPathCheckButton;
    private CheckButton _absProjPathCheckButton;
    private OptionButton _HUBBehaviorOptionButton;
    private Button _saveDataButton;

    public override void _Ready()
    {
        _fullExecPathCheckButton = GetNode<CheckButton>("%FullExecPathCheckButton");
        _absProjPathCheckButton = GetNode<CheckButton>("%AbsProjPathCheckButton");
        _HUBBehaviorOptionButton = GetNode<OptionButton>("%HUBBehaviorOptionButton");
        _saveDataButton = GetNode<Button>("%SaveDataButton");

        _fullExecPathCheckButton.Toggled += (bool state) => OnToggled(state, _fullExecPathCheckButton, "full_exec_path");
        _absProjPathCheckButton.Toggled += (bool state) => OnToggled(state, _absProjPathCheckButton, ABS_PROJ_PATH_TAG);
        _HUBBehaviorOptionButton.ItemSelected += _ => EmitSignal(SignalName.SettingChanged, HUB_BEHAVIOR_TAG);
        _saveDataButton.Pressed += () => OSAPI.OpenFolder(ProjectSettings.GlobalizePath("user://"));

    }

    public override string[] GetAllSettingTags() => [FULL_EXEC_PATH_TAG, ABS_PROJ_PATH_TAG, HUB_BEHAVIOR_TAG];

    public override SettingsData.Data GetData(string settingTag)
    {
        return settingTag switch
        {
            FULL_EXEC_PATH_TAG => _fullExecPathCheckButton.ButtonPressed,
            ABS_PROJ_PATH_TAG => _absProjPathCheckButton.ButtonPressed,
            HUB_BEHAVIOR_TAG => _HUBBehaviorOptionButton.Selected,
            _ => new()
        };
    }

    public override void SetData(string settingTag, SettingsData.Data data)
    {
        switch (settingTag)
        {
            case FULL_EXEC_PATH_TAG:
                _fullExecPathCheckButton.SetPressedNoSignal(data);
                if (_fullExecPathCheckButton.ButtonPressed)
                    _fullExecPathCheckButton.Text = "Full Path Shown";
                else
                    _fullExecPathCheckButton.Text = "Full Path Hidden";
                break;
            case ABS_PROJ_PATH_TAG:
                _absProjPathCheckButton.SetPressedNoSignal(data);
                if (_absProjPathCheckButton.ButtonPressed)
                    _absProjPathCheckButton.Text = "Full Path Shown";
                else
                    _absProjPathCheckButton.Text = "Full Path Hidden";
                break;
            case HUB_BEHAVIOR_TAG: _HUBBehaviorOptionButton.Select(data); break;
            default: break;
        }
    }

    private void OnToggled(bool state, CheckButton button, string tag)
    {
        if (state)
            button.Text = "Full Path Shown";
        else
            button.Text = "Full Path Hidden";

        EmitSignal(SignalName.SettingChanged, tag);
    }
}
