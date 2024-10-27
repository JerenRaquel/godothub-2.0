using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Projects : PanelContainer
{
    private PackedScene _projectEntryPackedScene;
    private Button _newButton;
    private Button _importButton;
    private Button _scanButton;
    private OptionButton _sortOptionButton;
    private CheckBox _checkBox;
    private LineEdit _filterLineEdit;
    private VBoxContainer _projectEntryContainer;

    public override void _ExitTree()
    {
        _scanButton.Pressed -= OnScanButtonPressed;
        _sortOptionButton.ItemSelected -= OnSortChanged;
        _checkBox.Toggled -= OnSortToggled;
        _filterLineEdit.TextChanged -= OnFilterChanged;
    }

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
        _projectEntryContainer = GetNode<VBoxContainer>("%ProjectEntryContainer");

        FillProjectContainer();
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
        ProjectCache.Instance.ScanProjects(["C:/Users/Jeren/Godot Projects"]);
        FillProjectContainer();
    }

    private void OnFilterChanged(string text)
    {
        foreach (ProjectEntry entry in _projectEntryContainer.GetChildren().Cast<ProjectEntry>())
        {
            if (text.Length == 0 || entry.Contains(text))
                entry.Show();
            else
                entry.Hide();
        }
    }

    private void OnSortChanged(long _index) => FillProjectContainer();

    private void OnSortToggled(bool toggled)
    {
        if (toggled)
            _checkBox.Text = "Descending";
        else
            _checkBox.Text = "Ascending";
        FillProjectContainer();
    }
}
