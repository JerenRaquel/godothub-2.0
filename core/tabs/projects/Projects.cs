using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class Projects : TabBase
{
    private PackedScene _projectEntryPackedScene;
    private Button _newButton;
    private Button _importButton;
    private Button _scanButton;
    private OptionButton _sortOptionButton;
    private CheckBox _checkBox;
    private LineEdit _filterLineEdit;
    private OptionButton _versionOptionButton;
    private VBoxContainer _projectEntryContainer;
    private ProjectSidePanel _sidePanel;
    private BuildPrompt _buildPrompt;

    private Dictionary<string, ProjectEntry> _projectEntries = [];

    public override void _Ready()
    {
        _projectEntryPackedScene = GD.Load<PackedScene>("res://core/tabs/projects/project_entry/project_entry.tscn");

        _newButton = GetNode<Button>("%NewButton");
        _importButton = GetNode<Button>("%ImportButton");
        _importButton.Pressed += OnImportPressed;
        _scanButton = GetNode<Button>("%ScanButton");
        _scanButton.Pressed += OnScanButtonPressed;
        _sortOptionButton = GetNode<OptionButton>("%SortOptionButton");
        _sortOptionButton.ItemSelected += OnSortChanged;
        _checkBox = GetNode<CheckBox>("%CheckBox");
        _checkBox.Toggled += OnSortToggled;
        _filterLineEdit = GetNode<LineEdit>("%FilterLineEdit");
        _filterLineEdit.TextChanged += OnFilterChanged;
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
        _versionOptionButton.ItemSelected += OnVersionChanged;
        _projectEntryContainer = GetNode<VBoxContainer>("%ProjectEntryContainer");
        _sidePanel = GetNode<ProjectSidePanel>("%ProjectSidePanel");
        _buildPrompt = GetNode<BuildPrompt>("%BuildPrompt");
        _buildPrompt.BuildUpdated += OnBuildUpdated;

        string[] versions = ProjectCache.Instance.GetVersions();
        Array.Sort(versions, VersionData.reverseComparer);
        foreach (string version in versions)
            _versionOptionButton.AddItem(version);

        FillProjectContainer();
    }

    public void UpdatePaths()
    {
        foreach (KeyValuePair<string, ProjectEntry> entry in _projectEntries)
        {
            if (entry.Value.IsQueuedForDeletion()) continue;

            entry.Value.UpdatePath();
        }
    }

    public void FillProjectContainer()
    {
        GD.Print("Resorted?");
        foreach (KeyValuePair<string, ProjectEntry> entry in _projectEntries)
        {
            if (entry.Value.IsQueuedForDeletion()) continue;
            entry.Value.QueueFree();
        }
        _projectEntries.Clear();

        List<string> projectNames = ProjectCache.Instance.ProjectNames;
        projectNames.Sort(CompareFunc);

        foreach (string projectName in projectNames)
        {
            ProjectEntry entryInstance = _projectEntryPackedScene.Instantiate<ProjectEntry>();
            _projectEntryContainer.AddChild(entryInstance);
            _projectEntries.Add(projectName, entryInstance);
            entryInstance.Initialize(projectName);
            entryInstance.LaunchRequested += OnLaunchRequested;
            entryInstance.Toggled += OnToggled;
        }
        Name = $"Projects [{ProjectCache.Instance.ProjectCount}]";
    }

    private void Filter()
    {
        string text = _filterLineEdit.Text;
        int versionIDX = _versionOptionButton.Selected;
        string version = _versionOptionButton.GetItemText(versionIDX);
        foreach (KeyValuePair<string, ProjectEntry> entry in _projectEntries)
        {
            if (text.Length == 0 || entry.Value.Contains(text))
                if (versionIDX == 0 || entry.Value.Contains(version))
                {
                    entry.Value.Show();
                    continue;
                }
            entry.Value.Hide();
        }
    }

    private int CompareFunc(string lhs, string rhs)
    {
        int result = 0;

        if (ProjectCache.Instance.IsFavorited(lhs) && !ProjectCache.Instance.IsFavorited(rhs))
            result = -1;
        else if (!ProjectCache.Instance.IsFavorited(lhs) && ProjectCache.Instance.IsFavorited(rhs))
            result = 1;
        else
        {
            if (_sortOptionButton.Selected == 0)  // Last Edited
            {
                if (ProjectCache.Instance.GetRawTime(lhs) < ProjectCache.Instance.GetRawTime(rhs))
                    result = -1;
                else if (ProjectCache.Instance.GetRawTime(lhs) > ProjectCache.Instance.GetRawTime(rhs))
                    result = 1;
                else
                    result = lhs.CompareTo(rhs);
            }
            else    // Alphabetical
                result = lhs.CompareTo(rhs);
        }

        if (_checkBox.ButtonPressed) return result;
        return result * -1;
    }

    private void OnScanButtonPressed()
    {
        string[] paths = SettingsCache.Instance.GetData("Project Settings/Paths/project_paths/STRING_LIST");
        ProjectCache.Instance.ScanProjects(paths);
        FillProjectContainer();
    }

    private void OnFilterChanged(string text) => Filter();

    private void OnVersionChanged(long index) => Filter();

    private void OnSortChanged(long _index) => FillProjectContainer();

    private void OnSortToggled(bool toggled)
    {
        if (toggled)
            _checkBox.Text = "Descending";
        else
            _checkBox.Text = "Ascending";
        FillProjectContainer();
    }

    private void OnLaunchRequested(string projectName)
    {
        if (ProjectCache.Instance.GetBuild(projectName) == VersionData.BuildType.UNKNOWN)
            _buildPrompt.Open(projectName);
        else
        {
            string key = ProjectCache.Instance.ProjectNameToKey(projectName);
            if (key != null)    // Success
            {
                string godotExe = VersionCache.Instance.GetPath(key);
                if (godotExe.Length > 0 && OSAPI.OpenGodotProject(godotExe, projectName) >= 0)
                {
                    NotifcationManager.Instance.NotifyValid("ProjectLaunching");
                    //? Is there a better way to fetch this key?
                    if (SettingsCache.Instance.GetData("Application/Config/HUB_behavior/LONG") == 0)
                    {
                        CallDeferred("FillProjectContainer");
                        return; //* Success
                    }
                    else
                    {
                        GetTree().Quit();
                        return;
                    }
                }
            }

            // Fail
            NotifcationManager.Instance.NotifyError("Failed to launch project.");
        }
    }

    private void OnToggled(string projectName, bool state)
    {
        if (_sidePanel.SelectedProject != null)
            _projectEntries[_sidePanel.SelectedProject].ToggleOff();

        if (state)
            _sidePanel.SetSelected(projectName);
        else
            _sidePanel.SetSelected("");
    }

    private void OnImportPressed()
    {
        FileDialogManager.Instance.Open("Locate Your Project", FileDialog.FileModeEnum.OpenFile, ["project.godot", ".gdhub"]);
        FileDialogManager.Instance.DataCompiled += OnImportFileLocated;
    }

    private void OnImportFileLocated(string path)
    {
        // TODO: Verify and add project
        GD.Print(path);
        FileDialogManager.Instance.DataCompiled -= OnImportFileLocated;
    }

    private void OnBuildUpdated(string projectName) => _projectEntries[projectName].UpdateProjectLabel();
}
