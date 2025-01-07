using Godot;
using System;

public partial class Software : TabBase
{
    [Signal] public delegate void EntryFavoritedEventHandler();

    [Export] private PackedScene entryScene;

    private Button _locateButton;
    private Button _editButton;
    private Button _launchButton;
    private LineEdit _filterLineEdit;
    private CheckBox _orderCheckBox;
    private Button _deleteButton;
    private VBoxContainer _container;
    private SoftwareLocator _softwareLocateWindow;

    private SoftwareEntry _currentlySelected = null;

    public override void _Ready()
    {
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnSoftwareAddPressed;
        _editButton = GetNode<Button>("%EditButton");
        _launchButton = GetNode<Button>("%LaunchButton");
        _launchButton.Pressed += OnLaunchRequested;
        _filterLineEdit = GetNode<LineEdit>("%FilterLineEdit");
        _orderCheckBox = GetNode<CheckBox>("%OrderCheckBox");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _container = GetNode<VBoxContainer>("%SoftwareContainer");
        _softwareLocateWindow = GetNode<SoftwareLocator>("%SoftwareLocator");
        _softwareLocateWindow.SoftwareLocated += AddEntry;

        ToggleEntryButtons(true);
        foreach (string tag in TagCache.Instance.SoftwareTags)
            AddEntry(tag);
    }

    private void ToggleEntryButtons(bool disabled)
    {
        _editButton.Disabled = disabled;
        _launchButton.Disabled = disabled;
        _deleteButton.Disabled = disabled;
    }

    private void AddEntry(string name)
    {
        SoftwareEntry entryInstance = entryScene.Instantiate<SoftwareEntry>();
        _container.AddChild(entryInstance);
        entryInstance.SetData(name);
        entryInstance.MainButton.StateToggled += (bool state) => OnStateToggled(state, entryInstance);
        entryInstance.MainButton.LaunchRequested += OnLaunchRequested;
        entryInstance.FavoriteToggled += () => EmitSignal(SignalName.EntryFavorited);
    }

    private void OnStateToggled(bool state, SoftwareEntry entry)
    {
        _currentlySelected?.MainButton.ToggleOff();

        if (state)
            _currentlySelected = entry;
        else
            _currentlySelected = null;

        ToggleEntryButtons(!state);
    }

    private void OnLaunchRequested()
    {
        string key = _currentlySelected.SoftwareTag;
        long processID = OSAPI.RunTool(key);
        if (processID == -1)
            NotifcationManager.Instance.NotifyError("Could not run software");
    }

    private void OnSoftwareAddPressed() => _softwareLocateWindow.Show();

}
