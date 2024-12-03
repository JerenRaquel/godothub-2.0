using Godot;
using System;

public partial class NewProjectWindow : WindowBase
{
    private const string FORWARD_TEXT = "* Supports desktop platforms only.\n* Advanced 3D graphics available.\n* Can scale to large complex scenes.\n* Uses RenderingDevice backend.\n* Slower rendering of simple scenes.";
    private const string MOBILE_TEXT = "* Supports desktop & mobile platforms.\n* Less advanced 3D graphics.\n* Less scalable for complex scenes.\n* Uses RenderingDevice backend.\n* Fast rendering of simple scenes.";
    private const string COMPAT_TEXT = "* Supports desktop, mobile & web platforms.\n* Least advanced 3D graphics (currently work-in-progress).\n* Intended for low-end/older devices.\n* Uses OpengGL 3 backend (OpengGL 3.3/ES 3.0/WebGL2).\n* Fastest rendering of simple scenes.";

    private LineEdit _nameLineEdit;
    private LineEdit _pathLineEdit;
    private OptionButton _pathOptionButton;
    private CheckBox _forwardCheckBox;
    private CheckBox _mobileCheckBox;
    private CheckBox _compatCheckBox;
    private Label _renderLabel;
    private OptionButton _versionOptionButton;
    private OptionButton _templateOptionButton;

    public override void _Ready()
    {
        _nameLineEdit = GetNode<LineEdit>("%NameLineEdit");
        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _pathOptionButton = GetNode<OptionButton>("%PathOptionButton");
        _forwardCheckBox = GetNode<CheckBox>("%ForwardCheckBox");
        _forwardCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _forwardCheckBox);
        _mobileCheckBox = GetNode<CheckBox>("%MobileCheckBox");
        _mobileCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _mobileCheckBox);
        _compatCheckBox = GetNode<CheckBox>("%CompatCheckBox");
        _compatCheckBox.Toggled += (bool state) => OnCheckBoxToggled(state, _compatCheckBox);
        _renderLabel = GetNode<Label>("%RenderLabel");
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
        _templateOptionButton = GetNode<OptionButton>("%TemplateOptionButton");

        RefreshKnownImportPaths();
        RefreshVersionOptions();
        RefreshTemplateOptions();
        base._Ready();
    }

    public void RefreshKnownImportPaths()
    {
        //? Is there a better way?
        string[] paths = SettingsCache.Instance.GetData("Project Settings/Paths/project_paths/STRING_LIST");
        if (paths.Length == 0)
        {
            _pathOptionButton.Hide();
            return;
        }

        _pathOptionButton.Clear();
        foreach (string path in paths)
            _pathOptionButton.AddItem(path);

        _pathOptionButton.Show();
    }

    public void RefreshVersionOptions()
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

    public void RefreshTemplateOptions()
    {
        _templateOptionButton.Clear();
        foreach (string key in TemplateCache.Instance.SortedTemplateNames)
        {
            _templateOptionButton.AddItem(key);
        }
    }

    protected override void ClearWindowData()
    {
        base.ClearWindowData();
        _nameLineEdit.Text = "";
        if (_pathOptionButton.ItemCount > 0)
        {
            _pathLineEdit.Text = _pathOptionButton.GetItemText(0);
            _pathOptionButton.Selected = 0;
            _pathOptionButton.Show();
        }
        else
        {
            _pathLineEdit.Text = "";
            _pathOptionButton.Hide();
        }
        _forwardCheckBox.SetPressedNoSignal(true);
        _mobileCheckBox.SetPressedNoSignal(false);
        _compatCheckBox.SetPressedNoSignal(false);
        _renderLabel.Text = FORWARD_TEXT;
        _versionOptionButton.Selected = 0;
        _templateOptionButton.Selected = 0;
    }

    protected override bool Validate()
    {
        return false;
    }

    protected override void OnConfirmPressed()
    {

    }

    private void OnCheckBoxToggled(bool state, CheckBox node)
    {
        if (!state)
        {
            node.SetPressedNoSignal(true);
            return;
        }

        if (node == _forwardCheckBox) _renderLabel.Text = FORWARD_TEXT;
        else _forwardCheckBox.SetPressedNoSignal(false);

        if (node == _mobileCheckBox) _renderLabel.Text = MOBILE_TEXT;
        else _mobileCheckBox.SetPressedNoSignal(false);

        if (node == _compatCheckBox) _renderLabel.Text = COMPAT_TEXT;
        else _compatCheckBox.SetPressedNoSignal(false);
    }

    private void OnProjectNameChanged(string text)
    {

        Validate();
    }
}
