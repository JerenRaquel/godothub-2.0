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
    private BuildPrompt _buildPrompt;

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
        _buildPrompt = GetNode<BuildPrompt>("%BuildPrompt");

        string[] versions = ProjectCache.Instance.GetVersions();
        Array.Sort(versions, new ReverseComparer());
        foreach (string version in versions)
            _versionOptionButton.AddItem(version);

        FillProjectContainer();
    }

    public void UpdatePaths()
    {
        foreach (ProjectEntry entry in _projectEntryContainer.GetChildren().Cast<ProjectEntry>())
        {
            if (entry.IsQueuedForDeletion()) continue;

            entry.UpdatePath();
        }
    }

    private void FillProjectContainer()
    {
        foreach (ProjectEntry entry in _projectEntryContainer.GetChildren().Cast<ProjectEntry>())
        {
            if (entry.IsQueuedForDeletion()) continue;
            entry.QueueFree();
        }

        List<string> projectNames = ProjectCache.Instance.ProjectNames;
        projectNames.Sort(CompareFunc);

        foreach (string projectName in projectNames)
        {
            ProjectEntry entryInstance = _projectEntryPackedScene.Instantiate<ProjectEntry>();
            _projectEntryContainer.AddChild(entryInstance);
            entryInstance.Initialize(projectName);
            entryInstance.LaunchRequested += OnLaunchRequested;
        }
    }

    private void Filter()
    {
        string text = _filterLineEdit.Text;
        int versionIDX = _versionOptionButton.Selected;
        string version = _versionOptionButton.GetItemText(versionIDX);
        foreach (ProjectEntry entry in _projectEntryContainer.GetChildren().Cast<ProjectEntry>())
        {
            if (text.Length == 0 || entry.Contains(text))
                if (versionIDX == 0 || entry.Contains(version))
                {
                    entry.Show();
                    continue;
                }
            entry.Hide();
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

}
