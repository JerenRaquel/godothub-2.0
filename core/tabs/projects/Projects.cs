using Godot;
using System;
using System.Linq;

public partial class Projects : PanelContainer
{
    private PackedScene _projectEntryPackedScene;
    private Button _newButton;
    private Button _importButton;
    private Button _scanButton;
    private CheckBox _checkBox;
    private LineEdit _filterLineEdit;
    private VBoxContainer _projectEntryContainer;

    public override void _ExitTree()
    {
        _scanButton.Pressed -= OnScanButtonPressed;
        _filterLineEdit.TextChanged -= OnFilterChanged;
    }

    public override void _Ready()
    {
        _projectEntryPackedScene = GD.Load<PackedScene>("res://core/tabs/projects/project_entry/project_entry.tscn");

        _newButton = GetNode<Button>("%NewButton");
        _importButton = GetNode<Button>("%ImportButton");
        _scanButton = GetNode<Button>("%ScanButton");
        _scanButton.Pressed += OnScanButtonPressed;
        _checkBox = GetNode<CheckBox>("%CheckBox");
        _filterLineEdit = GetNode<LineEdit>("%FilterLineEdit");
        _filterLineEdit.TextChanged += OnFilterChanged;
        _projectEntryContainer = GetNode<VBoxContainer>("%ProjectEntryContainer");

        FillProjectContainer();
    }

    private void FillProjectContainer()
    {
        foreach (string projectName in ProjectCache.Instance.ProjectNames)
        {
            ProjectEntry entryInstance = _projectEntryPackedScene.Instantiate<ProjectEntry>();
            _projectEntryContainer.AddChild(entryInstance);
            entryInstance.Initialize(projectName);
        }
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
}
