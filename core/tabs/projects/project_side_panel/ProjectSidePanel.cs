using System.Collections.Generic;
using Godot;

public partial class ProjectSidePanel : MarginContainer
{
    [Signal] public delegate void EditProjectEventHandler();
    [Signal] public delegate void DeleteProjectEventHandler();

    private Button _openButton;
    private Button _openWithOutToolsButton;
    private Button _runButton;
    private Button _openFolderButton;
    private Button _openSaveFolderButton;
    private Button _editButtonButton;
    private Button _cloneButton;
    private Button _deleteButton;
    private QuickToolRoot _quickToolRoot;

    private bool _isDisabled;

    public string SelectedProject { get; private set; } = null;
    public bool Disabled
    {
        get => _isDisabled;
        set => SetPanelState(value);
    }

    public override void _Ready()
    {
        _openButton = GetNode<Button>("%OpenButton");
        _openButton.Pressed += () => OpenProject(SelectedProject, true);

        _openWithOutToolsButton = GetNode<Button>("%OpenWOToolsButton");
        _openWithOutToolsButton.Pressed += () => OpenProject(SelectedProject, false);

        _runButton = GetNode<Button>("%RunButton");
        _runButton.Pressed += OnRunPressed;

        _openFolderButton = GetNode<Button>("%OpenFolderButton");
        _openFolderButton.Pressed += OnFolderOpenPressed;

        _openSaveFolderButton = GetNode<Button>("%OpenSaveFolderButton");
        _openSaveFolderButton.Pressed += OnSaveFolderOpenPressed;

        _editButtonButton = GetNode<Button>("%EditButton");
        _editButtonButton.Pressed += () => EmitSignal(SignalName.EditProject);

        _cloneButton = GetNode<Button>("%CloneButton");

        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += () => EmitSignal(SignalName.DeleteProject);

        _quickToolRoot = GetNode<QuickToolRoot>("%QuickToolRoot");

        SetPanelState(true);
        _quickToolRoot.SetQuickToolList();
    }

    public void SetSelected(string projectName)
    {
        if (projectName.Length == 0)
        {
            SelectedProject = null;
            SetPanelState(true);
        }
        else
        {
            SelectedProject = projectName;
            SetPanelState(false);
        }
    }

    private void SetPanelState(bool disabled)
    {
        _isDisabled = disabled;
        _openButton.Disabled = disabled;
        _openWithOutToolsButton.Disabled = disabled;
        _runButton.Disabled = disabled;

        if (!disabled && !ProjectCache.Instance.HasBuildSelected(SelectedProject))
            _editButtonButton.Disabled = true;
        else
            _editButtonButton.Disabled = disabled;

        _openFolderButton.Disabled = disabled;
        _openSaveFolderButton.Disabled = disabled;
        _cloneButton.Disabled = true;   // TEMP::UI is hidden -- Marked as future/planned
        _deleteButton.Disabled = disabled;
    }

    private void OnRunPressed() => RunProject(SelectedProject);

    private void OnFolderOpenPressed()
        => OSAPI.OpenFolder(ProjectCache.Instance.GetProjectFolder(SelectedProject));

    private void OnSaveFolderOpenPressed() => OSAPI.OpenUserFolder(SelectedProject);

    public static void RunProject(string projectName)
    {
        string key = ProjectCache.Instance.ProjectNameToKey(projectName);
        if (key == null)
        {
            // Failed
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
            return;
        }

        string godotExe = VersionCache.Instance.GetPath(key);
        if (godotExe.Length == 0)
        {
            // Failed
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
            return;
        }

        if (OSAPI.RunGodotProject(godotExe, projectName) == -1)
        {
            // Failed
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
            return;
        }
        NotifcationManager.Instance.NotifyValid("Project Running");
    }

    public static bool OpenProject(string projectName, bool withTools)
    {
        string key = ProjectCache.Instance.ProjectNameToKey(projectName);
        if (key == null)
        {
            // Failed
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
            return false;
        }

        string godotExe = VersionCache.Instance.GetPath(key);
        if (godotExe.Length == 0)
        {
            // Failed
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
            return false;
        }

        if (OSAPI.OpenGodotProject(godotExe, projectName) >= 0)
        {
            NotifcationManager.Instance.NotifyValid("Project Launching");

            if (withTools)
            {
                NotifcationManager.Instance.NotifyValid("Software Launching");
                foreach (string toolName in ProjectCache.Instance.GetSoftwareTags(projectName))
                    OSAPI.RunTool(toolName, projectName);
            }
            return true;
        }

        // Failed
        NotifcationManager.Instance.NotifyError("Failed to launch project.");
        return false;
    }

}
