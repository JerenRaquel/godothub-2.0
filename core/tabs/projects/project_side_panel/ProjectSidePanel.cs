using Godot;

public partial class ProjectSidePanel : MarginContainer
{
    private Button _openButton;
    private Button _openWithOutToolsButton;
    private Button _runButton;
    private Button _changeVersionButton;
    private Button _openFolderButton;
    private Button _renameButton;
    private Button _cloneButton;
    private Button _deleteButton;
    private VBoxContainer _quickToolRoot;

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
        _changeVersionButton = GetNode<Button>("%ChangeVerButton");
        _openFolderButton = GetNode<Button>("%OpenFolderButton");
        _renameButton = GetNode<Button>("%RenameButton");
        _cloneButton = GetNode<Button>("%CloneButton");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _quickToolRoot = GetNode<VBoxContainer>("%QuickToolRoot");

        SetPanelState(true);
        _quickToolRoot.Hide();
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
        _changeVersionButton.Disabled = disabled;
        _openFolderButton.Disabled = disabled;
        _renameButton.Disabled = disabled;
        _cloneButton.Disabled = disabled;
        _deleteButton.Disabled = disabled;
    }
}
