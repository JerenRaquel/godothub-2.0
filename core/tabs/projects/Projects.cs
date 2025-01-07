using Godot;
using System;
using System.Collections.Generic;

public partial class Projects : TabBase
{
    public const string VERSION_TAG = "GLOBAL/Projects/version_mode/LONG";
    public const string SORT_TAG = "GLOBAL/Projects/sort_mode/LONG";
    public const string SORT_MODIFIER_TAG = "GLOBAL/Projects/sort_modifier_mode/BOOL";

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
    private NewProjectWindow _newProjectPrompt;
    private EditProjectWindow _editProjectDataPrompt;
    private DeletePrompt _deletePrompt;

    private Dictionary<string, ProjectEntry> _projectEntries = [];

    public override void _ExitTree()
    {
        SettingsCache.Instance.AddOrUpdate(VERSION_TAG, _versionOptionButton.Selected);
        SettingsCache.Instance.AddOrUpdate(SORT_TAG, _sortOptionButton.Selected);
        SettingsCache.Instance.AddOrUpdate(SORT_MODIFIER_TAG, _checkBox.ButtonPressed);
    }

    public override void _Ready()
    {
        _projectEntryPackedScene = GD.Load<PackedScene>("res://core/tabs/projects/project_entry/project_entry.tscn");

        _newButton = GetNode<Button>("%NewButton");
        _newButton.Pressed += OnNewProjectPressed;
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
        _sidePanel.EditProject += OnEditProjectPressed;
        _sidePanel.DeleteProject += OnDeleteProjectPressed;
        _buildPrompt = GetNode<BuildPrompt>("%BuildPrompt");
        _buildPrompt.BuildUpdated += OnBuildUpdated;
        _newProjectPrompt = GetNode<NewProjectWindow>("%NewProjectWindow");
        _editProjectDataPrompt = GetNode<EditProjectWindow>("%EditProjectWindow");
        _editProjectDataPrompt.BuildUpdated += OnBuildUpdated;
        _deletePrompt = GetNode<DeletePrompt>("%DeletePrompt");
        _deletePrompt.ProjectDeletedSuccessfully += OnProjectDeletedSuccessfully;

        string[] versions = ProjectCache.Instance.GetVersions();
        Array.Sort(versions, VersionData.reverseComparer);
        foreach (string version in versions)
            _versionOptionButton.AddItem(version);

        FillProjectContainer();
    }

    public override void LoadData()
    {
        _versionOptionButton.Selected = SettingsCache.Instance.GetDataOrSetDefault(VERSION_TAG, new(0));
        _sortOptionButton.Selected = SettingsCache.Instance.GetDataOrSetDefault(SORT_TAG, new(0));
        _checkBox.ButtonPressed = SettingsCache.Instance.GetDataOrSetDefault(SORT_MODIFIER_TAG, new(false));
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
            entryInstance.DoubleClickButton.LaunchRequested += () => OnLaunchRequested(projectName);
            entryInstance.DoubleClickButton.StateToggled += (bool state) => OnToggled(projectName, state);
            entryInstance.EntryFavoriteToggled += FillProjectContainer;
        }
        Name = $"Projects [{ProjectCache.Instance.ProjectCount}]";
    }

    public void UpdateQuickTools() => _sidePanel.UpdateQuickTools();

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
        int result;
        if (ProjectCache.Instance.IsFavorited(lhs) && !ProjectCache.Instance.IsFavorited(rhs))
            result = 1;
        else if (!ProjectCache.Instance.IsFavorited(lhs) && ProjectCache.Instance.IsFavorited(rhs))
            result = -1;
        else
        {
            if (_sortOptionButton.Selected == 0)  // Last Edited
            {
                if (ProjectCache.Instance.GetRawTime(lhs) < ProjectCache.Instance.GetRawTime(rhs))
                    result = -1;
                else if (ProjectCache.Instance.GetRawTime(lhs) > ProjectCache.Instance.GetRawTime(rhs))
                    result = 1;
                else
                    result = lhs.CompareTo(rhs) * -1;
            }
            else    // Alphabetical
                result = lhs.CompareTo(rhs) * -1;
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

    private void OnVersionChanged(long index)
    {
        SettingsCache.Instance.AddOrUpdate(VERSION_TAG, index);
        Filter();
    }

    private void OnSortChanged(long index)
    {
        SettingsCache.Instance.AddOrUpdate(SORT_TAG, index);
        FillProjectContainer();
    }

    private void OnSortToggled(bool toggled)
    {
        if (toggled)
            _checkBox.Text = "Descending";
        else
            _checkBox.Text = "Ascending";
        SettingsCache.Instance.AddOrUpdate(SORT_MODIFIER_TAG, toggled);
        FillProjectContainer();
    }

    private void OnLaunchRequested(string projectName)
    {
        if (ProjectCache.Instance.GetBuild(projectName) == VersionData.BuildType.UNKNOWN)
        {
            _buildPrompt.Open(projectName);
            return;
        }

        //? Is there a better way to fetch this key?
        int runInstruction = SettingsCache.Instance.GetData("Project Settings/Defaults/launch_behavior/LONG");
        if (runInstruction == 2)  // Run
        {
            ProjectSidePanel.RunProject(projectName);
            return;
        }

        if (ProjectSidePanel.OpenProject(projectName, runInstruction == 1))
        {
            if (SettingsCache.Instance.GetData("Application/Config/HUB_behavior/LONG") == 0)
            {
                // Resort
                CallDeferred("FillProjectContainer");
                return; //* Success
            }
            GetTree().Quit();
        }
    }

    private void OnToggled(string projectName, bool state)
    {
        if (_sidePanel.SelectedProject != null)
            _projectEntries[_sidePanel.SelectedProject].DoubleClickButton.ToggleOff();

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

    private void OnNewProjectPressed() => _newProjectPrompt.Show();

    private void OnEditProjectPressed() => _editProjectDataPrompt.Open(_sidePanel.SelectedProject);

    private void OnDeleteProjectPressed() => _deletePrompt.Open(_sidePanel.SelectedProject);

    private void OnProjectDeletedSuccessfully() => FillProjectContainer();
}
