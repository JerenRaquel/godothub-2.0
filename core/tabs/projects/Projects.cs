using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class Projects : PanelContainer
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.array.sort?view=net-8.0
    public class ReverseComparer : IComparer
    {
        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer.Compare(object x, object y)
        {
            return new CaseInsensitiveComparer().Compare(y, x);
        }
    }

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

        string[] versions = ProjectCache.Instance.GetVersions();
        Array.Sort(versions, new ReverseComparer());
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

    private void FillProjectContainer()
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
            entryInstance.LaunchRequested += OnLaunchRequested;
            entryInstance.Toggled += OnToggled;
        }
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

    private void OnLaunchRequested(string projectName) => _buildPrompt.Open(projectName);

    private void OnToggled(string projectName, bool state)
    {
        if (_sidePanel.SelectedProject != null)
            _projectEntries[_sidePanel.SelectedProject].ToggleOff();

        if (state)
            _sidePanel.SetSelected(projectName);
        else
            _sidePanel.SetSelected("");
    }

}
