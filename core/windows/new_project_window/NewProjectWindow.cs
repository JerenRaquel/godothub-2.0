using Godot;
using System;
using System.IO;

public partial class NewProjectWindow : WindowBase
{
    private const string FORWARD_TEXT = "* Supports desktop platforms only.\n* Advanced 3D graphics available.\n* Can scale to large complex scenes.\n* Uses RenderingDevice backend.\n* Slower rendering of simple scenes.";
    private const string MOBILE_TEXT = "* Supports desktop & mobile platforms.\n* Less advanced 3D graphics.\n* Less scalable for complex scenes.\n* Uses RenderingDevice backend.\n* Fast rendering of simple scenes.";
    private const string COMPAT_TEXT = "* Supports desktop, mobile & web platforms.\n* Least advanced 3D graphics (currently work-in-progress).\n* Intended for low-end/older devices.\n* Uses OpengGL 3 backend (OpengGL 3.3/ES 3.0/WebGL2).\n* Fastest rendering of simple scenes.";

    [Signal] public delegate void ProjectCreatedEventHandler();

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
        _nameLineEdit.TextChanged += OnProjectNameChanged;
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
        _renderLabel.Text = FORWARD_TEXT;
        _versionOptionButton.Selected = 0;
        _templateOptionButton.Selected = 0;
    }

    protected override bool Validate()
    {
        string path = _pathLineEdit.Text.StripEdges();
        bool isWarning = false;

        if (Directory.Exists(path))
        {
            if (File.Exists(path + "/.gdhub") || File.Exists(path + "/project.godot"))
            {
                DisplayError("Project already exists.");
                return false;
            }
            else if (!OSAPI.IsDirectoryEmpty(path))
            {

                DisplayWarning("Directory is not empty.");
                isWarning = true;
            }
        }

        if (!isWarning) DisplayMessage("Valid Project");
        return true;
    }

    protected override void OnOpened()
    {
        int idx = SettingsCache.Instance.GetData("Project Settings/Defaults/rendering_device/LONG");
        switch (idx)
        {
            case 0: OnCheckBoxToggled(true, _forwardCheckBox); break;
            case 1: OnCheckBoxToggled(true, _mobileCheckBox); break;
            case 2: OnCheckBoxToggled(true, _compatCheckBox); break;
            default: OnCheckBoxToggled(true, _forwardCheckBox); break;
        }
    }

    protected override void OnConfirmPressed()
    {
        string projectPath = _pathLineEdit.Text.StripEdges();
        string templateName = _templateOptionButton.GetItemText(_templateOptionButton.Selected);
        if (!TemplateCache.Instance.HasTemplate(templateName))
        {
            NotifcationManager.Instance.NotifyError("Invalid template used. Some how this got past the validation step...");
            Hide();
            return;
        }

        string godotVersionStrData = _versionOptionButton.GetItemText(_versionOptionButton.Selected);
        bool isCSharp = godotVersionStrData.Contains("[.Net]");
        string versionStr = ParseVersionStr(godotVersionStrData);
        if (versionStr == null)
        {
            NotifcationManager.Instance.NotifyError("Could not determine version...");
            Hide();
            return;
        }

        string rendererStr = GetRendererString();
        if (rendererStr == null)
        {
            NotifcationManager.Instance.NotifyError("Could not determine renderer...");
            Hide();
            return;
        }

        string projectName = FormatFolderName(_nameLineEdit.Text.StripEdges());
        if (rendererStr == null)
        {
            NotifcationManager.Instance.NotifyError("Project name not valid. Some how this got past the validation step...");
            Hide();
            return;
        }

        VersionData.BuildType buildType = ParseBuildStr(godotVersionStrData);
        if (buildType == VersionData.BuildType.UNKNOWN)
        {
            NotifcationManager.Instance.NotifyError("Build type not valid. Some how this got past the validation step...");
            Hide();
            return;
        }

        ProjectCreator.ProjectCreationData data = new(projectName, isCSharp, rendererStr, versionStr);
        bool successState = ProjectCreator.CreateProject(projectPath, templateName, data);

        // Cache Project Data
        if (successState)
        {
            ProjectCache.Instance.AddProject(data, projectPath, templateName, buildType);
            EmitSignal(SignalName.ProjectCreated);
        }

        Hide();
    }

    private string GetRendererString()
    {
        //* This is done this way so ensure a single spot where the string conversion is made.
        if (_forwardCheckBox.ButtonPressed)
            return ProjectCache.RenderEnumToString(ProjectData.Renderer.FORWARD);
        else if (_mobileCheckBox.ButtonPressed)
            return ProjectCache.RenderEnumToString(ProjectData.Renderer.MOBILE);
        else if (_compatCheckBox.ButtonPressed)
            return ProjectCache.RenderEnumToString(ProjectData.Renderer.COMPAT);
        else
            return null;
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
            _renderLabel.Text = FORWARD_TEXT;
            _forwardCheckBox.SetPressedNoSignal(true);
        }
        else _forwardCheckBox.SetPressedNoSignal(false);

        if (node == _mobileCheckBox)
        {
            _renderLabel.Text = MOBILE_TEXT;
            _mobileCheckBox.SetPressedNoSignal(true);
        }
        else _mobileCheckBox.SetPressedNoSignal(false);

        if (node == _compatCheckBox)
        {
            _renderLabel.Text = COMPAT_TEXT;
            _compatCheckBox.SetPressedNoSignal(true);
        }
        else _compatCheckBox.SetPressedNoSignal(false);
    }

    private void OnProjectNameChanged(string text)
    {
        _pathLineEdit.Text = _pathOptionButton.GetItemText(_pathOptionButton.Selected);
        if (text.Length > 0)
        {
            string name = FormatFolderName(text);
            if (name == null)
            {
                DisplayError("Invalid Folder Name");
                return;
            }

            _pathLineEdit.Text += "/" + name;
        }

        Validate();
    }

    private static string FormatFolderName(string name)
    {
        int idx = SettingsCache.Instance.GetData("Project Settings/Defaults/naming_scheme/LONG");
        string folderName = idx switch
        {
            0 => name.Replace("-", " ").Replace("_", " ").ToPascalCase(),  // PascalCase
            1 => name.Replace("-", " ").ToSnakeCase(),  // snake_case
            2 => name.ToSnakeCase().Replace("_", "-"),  // kebab-case
            3 => name.Replace("-", " ").Replace("_", " ").ToCamelCase(),    // camelCase
            _ => name
        };

        if (OSAPI.IsValidFolderName(folderName)) return folderName;
        return null;
    }

    private static string ParseVersionStr(string dataStr)
    {
        string[] parts = dataStr.Split(" [", false);
        if (parts.Length == 0) return null;

        return parts[0].Replace("v", "");
    }

    private static VersionData.BuildType ParseBuildStr(string dataStr)
    {
        string[] parts = dataStr.Split(" [", false);
        if (parts.Length == 0) return VersionData.BuildType.UNKNOWN;

        string newDataStr = parts[1];
        string[] newParts = newDataStr.Split("]", false);
        string buildStr = newParts[0];
        return VersionData.StringToBuildEnum(buildStr);
    }
}
