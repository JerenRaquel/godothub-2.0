using System.Collections.Generic;
using Godot;

public partial class ProjectSidePanel : MarginContainer
{
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
        _openWithOutToolsButton = GetNode<Button>("%OpenWOToolsButton");
        _runButton = GetNode<Button>("%RunButton");
        _editButtonButton = GetNode<Button>("%EditButton");
        _openFolderButton = GetNode<Button>("%OpenFolderButton");
        _openFolderButton.Pressed += OnFolderOpenPressed;
        _openSaveFolderButton = GetNode<Button>("%OpenSaveFolderButton");
        _openSaveFolderButton.Pressed += OnSaveFolderOpenPressed;
        _cloneButton = GetNode<Button>("%CloneButton");
        _deleteButton = GetNode<Button>("%DeleteButton");
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
        _editButtonButton.Disabled = true;   // TEMP
        _openFolderButton.Disabled = disabled;
        _openSaveFolderButton.Disabled = disabled;
        _cloneButton.Disabled = true;   // TEMP
        _deleteButton.Disabled = true;   // TEMP
    }

    private void OnFolderOpenPressed()
        => OSAPI.OpenFolder(ProjectCache.Instance.GetProjectFolder(SelectedProject));

    private void OnSaveFolderOpenPressed() => OSAPI.OpenUserFolder(SelectedProject);
}
