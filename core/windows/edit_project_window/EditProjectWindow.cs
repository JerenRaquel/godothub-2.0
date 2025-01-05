using Godot;
using System;

public partial class EditProjectWindow : WindowBase
{
    [Signal] public delegate void BuildUpdatedEventHandler(string projectName);

    private const string FORWARD_TEXT = "* Supports desktop platforms only.\n* Advanced 3D graphics available.\n* Can scale to large complex scenes.\n* Uses RenderingDevice backend.\n* Slower rendering of simple scenes.";
    private const string MOBILE_TEXT = "* Supports desktop & mobile platforms.\n* Less advanced 3D graphics.\n* Less scalable for complex scenes.\n* Uses RenderingDevice backend.\n* Fast rendering of simple scenes.";
    private const string COMPAT_TEXT = "* Supports desktop, mobile & web platforms.\n* Least advanced 3D graphics (currently work-in-progress).\n* Intended for low-end/older devices.\n* Uses OpengGL 3 backend (OpengGL 3.3/ES 3.0/WebGL2).\n* Fastest rendering of simple scenes.";

    private CheckBox _forwardCheckBox;
    private CheckBox _mobileCheckBox;
    private CheckBox _compatCheckBox;
    private Label _renderInfoLabel;
    private OptionButton _versionOptionButton;

    private string _cachedRenderTypeStr;
    private int _cachedVersionBuildIndex;
    private string _cachedProjectName;

    public override void _Ready()
    {
        base._Ready();

        _forwardCheckBox = GetNode<CheckBox>("%ForwardCheckBox");
        _forwardCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _forwardCheckBox);
        _mobileCheckBox = GetNode<CheckBox>("%MobileCheckBox");
        _mobileCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _mobileCheckBox);
        _compatCheckBox = GetNode<CheckBox>("%CompatCheckBox");
        _compatCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _compatCheckBox);
        _renderInfoLabel = GetNode<Label>("%RenderLabel");
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
        _versionOptionButton.ItemSelected += OnOptionUpdated;
    }

    public void Open(string projectName)
    {
        _cachedProjectName = projectName;
        _cachedRenderTypeStr = ProjectCache.Instance.GetRenderer(projectName);
        GD.Print(_cachedRenderTypeStr);
        switch (_cachedRenderTypeStr)
        {
            case "Compatibility":
                OnCheckBoxToggled(true, _compatCheckBox);
                break;
            case "Mobile":
                OnCheckBoxToggled(true, _mobileCheckBox);
                break;
            case "Forward+":
            default:
                OnCheckBoxToggled(true, _forwardCheckBox);
                break;
        }

        RefreshVersionOptions();
        string versionBuild = ProjectCache.Instance.GetProjectVersionBuild(projectName);
        if (versionBuild == null) return;

        for (int i = 0; i < _versionOptionButton.ItemCount; i++)
        {
            if (_versionOptionButton.GetItemText(i) == versionBuild)
            {
                _cachedVersionBuildIndex = i;
                _versionOptionButton.Select(i);
                break;
            }
        }
        Validate();

        Show();
    }

    protected override bool Validate()
    {
        bool isRendererSame = false;
        if (_compatCheckBox.ButtonPressed && _cachedRenderTypeStr == "Compatibility")
            isRendererSame = true;
        else if (_mobileCheckBox.ButtonPressed && _cachedRenderTypeStr == "Mobile")
            isRendererSame = true;
        else if (_forwardCheckBox.ButtonPressed && _cachedRenderTypeStr == "Forward+")
            isRendererSame = true;

        if (_cachedVersionBuildIndex == _versionOptionButton.Selected && isRendererSame)
        {
            DisplayError("No changes were made.");
            return false;
        }

        DisplayMessage("Changes are valid.");
        return true;
    }

    protected override void OnConfirmPressed()
    {
        if (Validate())
        {
            VersionData.BuildType build = VersionData.StringToBuildEnum(_versionOptionButton.GetItemText(_versionOptionButton.Selected));
            ProjectCache.Instance.SetBuild(_cachedProjectName, build);
            EmitSignal(SignalName.BuildUpdated, _cachedProjectName);
        }

        ClearWindowData();
        Hide();
    }

    private void RefreshVersionOptions()
    {
        _versionOptionButton.Clear();
        foreach (string key in VersionCache.Instance.SortedKeys)
        {
            VersionData.ParsedVersionKey parts = VersionData.ParseKey(key);
            string optionStr = $"v{parts.version} [{VersionData.BuildEnumToString(parts.build)}]";
            if (parts.isCSharp)
                optionStr += $" [.Net]";

            _versionOptionButton.AddItem(optionStr);
        }
    }

    private void OnCheckBoxToggled(bool state, CheckBox node)
    {
        if (!state)
        {
            node.SetPressedNoSignal(true);
            return;
        }

        if (node == _forwardCheckBox)
        {
            _renderInfoLabel.Text = FORWARD_TEXT;
            _forwardCheckBox.SetPressedNoSignal(true);
        }
        else _forwardCheckBox.SetPressedNoSignal(false);

        if (node == _mobileCheckBox)
        {
            _renderInfoLabel.Text = MOBILE_TEXT;
            _mobileCheckBox.SetPressedNoSignal(true);
        }
        else _mobileCheckBox.SetPressedNoSignal(false);

        if (node == _compatCheckBox)
        {
            _renderInfoLabel.Text = COMPAT_TEXT;
            _compatCheckBox.SetPressedNoSignal(true);
        }
        else _compatCheckBox.SetPressedNoSignal(false);
        Validate();
    }
}
