using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotVersions : PanelContainer
{
    private readonly VersionData.BuildType[] BUILD_MAP = [
        VersionData.BuildType.UNKNOWN,
        VersionData.BuildType.DEV,
        VersionData.BuildType.BETA,
        VersionData.BuildType.RELEASE_CANDIDATE,
        VersionData.BuildType.STABLE
    ];

    [Export] private PackedScene card;

    private Timer _doubleClickTimer;
    private Button _locateButton;
    private Button _openFolderButton;
    private Button _runButton;
    private OptionButton _languageOptionButton;
    private OptionButton _buildOptionButton;
    private Button _deleteButton;
    private GridContainer _cardGrid;
    private LocateGodotWindow _locateWindow;

    private Dictionary<Control, string> _versions = [];
    private Control _currentlySelected = null;

    public override void _Ready()
    {
        _doubleClickTimer = GetNode<Timer>("%DoubleClickTimer");
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnLocatePressed;
        _openFolderButton = GetNode<Button>("%OpenLocationButton");
        _openFolderButton.Pressed += OnFolderOpenPressed;
        _runButton = GetNode<Button>("%RunButton");
        _runButton.Pressed += OnLaunchPressed;
        _languageOptionButton = GetNode<OptionButton>("%LangOptionButton");
        _languageOptionButton.ItemSelected += OnOptionChanged;
        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _buildOptionButton.ItemSelected += OnOptionChanged;
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += OnDeletePressed;
        _cardGrid = GetNode<GridContainer>("%CardViewContainer");
        _locateWindow = GetNode<LocateGodotWindow>("%LocateGodotWindow");
        _locateWindow.VersionLocated += OnVersionLocated;

        ToggleEntryButtons(false);
        RefreshEntries();
    }

    private void RefreshEntries()
    {
        foreach (string key in VersionCache.Instance.SortedKeys)
            OnVersionLocated(key);
    }

    private void Filter()
    {
        // TODO: Determine if we are in card or list view
        foreach (IGodotVersionEntryInterface item in _cardGrid.GetChildren().Cast<IGodotVersionEntryInterface>())
        {
            if (_languageOptionButton.Selected == 1)    // Only GDScript
            {
                if (item.HasCSharp())
                {
                    ((Control)item).Hide();
                    continue;
                }
            }
            else if (_languageOptionButton.Selected == 2)   // .Net
            {
                if (!item.HasCSharp())
                {
                    ((Control)item).Hide();
                    continue;
                }
            }

            if (_buildOptionButton.Selected != 0)   // Is not on any
            {
                // Doesn't have the build
                if (!item.HasBuild(BUILD_MAP[_buildOptionButton.Selected]))
                {
                    ((Control)item).Hide();
                    continue;
                }
            }

            ((Control)item).Show();
        }
    }

    private Card AddCard(string version, VersionData.BuildType build, bool isCSharp)
    {
        Card cardInstance = card.Instantiate<Card>();
        _cardGrid.AddChild(cardInstance);
        cardInstance.SetData(version, build, isCSharp);
        cardInstance.Toggled += (bool state) => OnEntryToggled(state, cardInstance);
        return cardInstance;
    }

    private void EvalDoubleClick()
    {
        if (_doubleClickTimer.TimeLeft > 0)
        {
            _doubleClickTimer.Stop();
            OnFolderOpenPressed();
        }
    }

    private void ToggleEntryButtons(bool disabled)
    {
        _openFolderButton.Disabled = disabled;
        _runButton.Disabled = disabled;
        _deleteButton.Disabled = disabled;
    }

    private void OnLocatePressed() => _locateWindow.Show();

    private void OnVersionLocated(string key)
    {
        // TODO: Determine which version to spawn based on display

        // TEMP: Remove once TODO^ is compete
        VersionData.ParsedVersionKey parts = VersionData.ParseKey(key);
        Control card = AddCard(parts.version.ToString(), parts.build, parts.isCSharp);
        _versions.Add(card, key);
    }

    private void OnOptionChanged(long _index) => Filter();

    private void OnDeletePressed()
    {
        string key = _versions[_currentlySelected];
        if (!VersionCache.Instance.RemoveVersion(key)) return;  // Fail removal

        // Success
        _versions.Remove(_currentlySelected);
        _currentlySelected.QueueFree();
        _currentlySelected = null;
        ToggleEntryButtons(true);
    }

    private void OnEntryToggled(bool state, Control entry)
    {
        (_currentlySelected as IGodotVersionEntryInterface)?.ToggleOff();

        EvalDoubleClick();
        if (state)
            _currentlySelected = entry;
        else
            _currentlySelected = null;
        ToggleEntryButtons(!state);
        _doubleClickTimer.Start();
    }

    private void OnLaunchPressed()
    {
        GD.Print("Version Launch");
        // TODO: Finish -- Connect with OS System
    }

    private void OnFolderOpenPressed()
    {
        GD.Print("Version Directory Open");
        // TODO: Finish -- Connect with OS System
    }
}
