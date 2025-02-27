using Godot;
using System;
using System.Collections.Generic;

public partial class Projects : TabBase
{
    public const string VERSION_TAG = "GLOBAL/Projects/version_mode/LONG";
    public const string SORT_TAG = "GLOBAL/Projects/sort_mode/LONG";
    public const string SORT_MODIFIER_TAG = "GLOBAL/Projects/sort_modifier_mode/BOOL";

    [Signal] public delegate void GoToVersionsRequestedEventHandler();

    private PackedScene _projectEntryPackedScene;
    private Button _newButton;
    private Button _importButton;
    private Button _scanButton;
    private OptionButton _sortOptionButton;
    private CheckBox _checkBox;
    private LineEdit _filterLineEdit;
    private OptionButton _versionOptionButton;
    private VBoxContainer _projectEntryContainer;

    private VBoxContainer _labelContainer;
    private Label _noGodotVersionLabel;
    private Label _noProjectLabel;

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

        _labelContainer = GetNode<VBoxContainer>("%LabelContainer");

        _noGodotVersionLabel = GetNode<Label>("%NoGodotVersionLabel");

        _noProjectLabel = GetNode<Label>("%NoProjectsLabel");

        _sidePanel = GetNode<ProjectSidePanel>("%ProjectSidePanel");
        _sidePanel.EditProject += OnEditProjectPressed;
        _sidePanel.DeleteProject += OnDeleteProjectPressed;

        _buildPrompt = GetNode<BuildPrompt>("%BuildPrompt");
        _buildPrompt.BuildUpdated += OnBuildUpdated;
        _buildPrompt.LaunchConfirmed += OnLaunchOnConfirm;
        _buildPrompt.GoToVersionRequested += () => EmitSignal(SignalName.GoToVersionsRequested);

        _newProjectPrompt = GetNode<NewProjectWindow>("%NewProjectWindow");
        _newProjectPrompt.ProjectCreated += FillProjectContainer;

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

        if (VersionCache.Instance.Count == 0)
        {
            _labelContainer.Show();
            _noGodotVersionLabel.Show();
            _noProjectLabel.Hide();
            ToggleToolBar(false);
            return;
        }

        ToggleToolBar(true);
        List<string> projectNames = ProjectCache.Instance.ProjectNames;
        if (projectNames.Count == 0)
        {
            _labelContainer.Show();
            _noGodotVersionLabel.Hide();
            _noProjectLabel.Show();
            return;
        }

        _labelContainer.Hide();
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

    private void ToggleToolBar(bool state)
    {
        _newButton.Disabled = !state;
        _importButton.Disabled = !state;
        _scanButton.Disabled = !state;
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
        OnLaunchOnConfirm(projectName);
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
        switch (ProjectCache.Instance.ImportProject(path))
        {
            case ProjectCache.ImportError.OK:
                FillProjectContainer();
                NotifcationManager.Instance.NotifyValid("Project imported!");
                break;
            case ProjectCache.ImportError.PROJECT_DUPLICATE:
                NotifcationManager.Instance.NotifyError("Project is already imported.");
                break;
            case ProjectCache.ImportError.PROJECT_READ_FAIL:
            default:
                NotifcationManager.Instance.NotifyError("Unable to import project.");
                break;

        }
        FileDialogManager.Instance.DataCompiled -= OnImportFileLocated;
    }

    private void OnBuildUpdated(string projectName) => _projectEntries[projectName].UpdateProjectLabel();

    private void OnLaunchOnConfirm(string projectName)
    {
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

    private void OnNewProjectPressed() => _newProjectPrompt.Show();

    private void OnEditProjectPressed() => _editProjectDataPrompt.Open(_sidePanel.SelectedProject);

    private void OnDeleteProjectPressed() => _deletePrompt.Open(_sidePanel.SelectedProject);

    private void OnProjectDeletedSuccessfully() => FillProjectContainer();
}
