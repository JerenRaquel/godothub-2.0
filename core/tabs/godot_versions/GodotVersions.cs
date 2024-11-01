using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotVersions : TabBase
{
    public const string VIEW_TAG = "GLOBAL/GodotVersion/view_mode/BOOL";
    public const string LANGUAGE_TAG = "GLOBAL/GodotVersion/lang_support_mode/LONG";
    public const string RELEASE_TAG = "GLOBAL/GodotVersion/release_mode/LONG";

    private readonly VersionData.BuildType[] BUILD_MAP = [
        VersionData.BuildType.UNKNOWN,
        VersionData.BuildType.DEV,
        VersionData.BuildType.BETA,
        VersionData.BuildType.RELEASE_CANDIDATE,
        VersionData.BuildType.STABLE
    ];

    [Export] private PackedScene cardEntry;
    [Export] private PackedScene listEntry;

    private Button _locateButton;
    private Button _openFolderButton;
    private Button _runButton;
    private OptionButton _languageOptionButton;
    private OptionButton _buildOptionButton;
    private CheckButton _viewCheckButton;
    private Button _deleteButton;
    private ScrollContainer _listView;
    private ScrollContainer _cardView;
    private VBoxContainer _listContainer;
    private GridContainer _cardGrid;
    private LocateGodotWindow _locateWindow;

    private Dictionary<VersionEntryBase, string> _versions = [];
    private VersionEntryBase _currentlySelected = null;

    public override void _ExitTree() => SettingsCache.Instance.AddOrUpdate(VIEW_TAG, _viewCheckButton.ButtonPressed);

    public override void _Ready()
    {
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnLocatePressed;
        _openFolderButton = GetNode<Button>("%OpenLocationButton");
        _openFolderButton.Pressed += OnFolderOpenPressed;
        _runButton = GetNode<Button>("%RunButton");
        _runButton.Pressed += OnLaunchPressed;
        _languageOptionButton = GetNode<OptionButton>("%LangOptionButton");
        _languageOptionButton.ItemSelected += (long index) => OnOptionChanged(index, LANGUAGE_TAG);
        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _buildOptionButton.ItemSelected += (long index) => OnOptionChanged(index, RELEASE_TAG);
        _viewCheckButton = GetNode<CheckButton>("%ViewCheckButton");
        _viewCheckButton.Toggled += OnViewToggled;
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += OnDeletePressed;
        _listView = GetNode<ScrollContainer>("%ListView");
        _cardView = GetNode<ScrollContainer>("%CardView");
        _listContainer = GetNode<VBoxContainer>("%ListViewContainer");
        _cardGrid = GetNode<GridContainer>("%CardViewContainer");
        _locateWindow = GetNode<LocateGodotWindow>("%LocateGodotWindow");
        _locateWindow.VersionLocated += OnVersionLocated;

        ToggleEntryButtons(true);
    }

    public override void LoadData()
    {
        _viewCheckButton.SetPressedNoSignal(SettingsCache.Instance.GetDataOrSetDefault(VIEW_TAG, new(true)));
        _languageOptionButton.Selected = SettingsCache.Instance.GetDataOrSetDefault(LANGUAGE_TAG, new(0));
        _buildOptionButton.Selected = SettingsCache.Instance.GetDataOrSetDefault(RELEASE_TAG, new(0));

        RefreshEntries();
    }

    private void RefreshEntries()
    {
        foreach (KeyValuePair<VersionEntryBase, string> entry in _versions)
        {
            if (entry.Key.IsQueuedForDeletion()) continue;

            entry.Key.QueueFree();
        }
        _versions = [];
        _currentlySelected?.DoubleClickButton.ToggleOff();
        _currentlySelected = null;

        if (SettingsCache.Instance.GetData(VIEW_TAG))
        {
            _listView.Hide();
            _cardView.Show();
        }
        else
        {
            _listView.Show();
            _cardView.Hide();
        }

        foreach (string key in VersionCache.Instance.SortedKeys)
            OnVersionLocated(key);
        Filter();
    }

    private void Filter()
    {
        foreach (KeyValuePair<VersionEntryBase, string> entry in _versions)
        {
            if (_languageOptionButton.Selected == 1 && entry.Key.IsCSharp)    // Only GDScript
            {
                entry.Key.Hide();
                continue;
            }
            else if (_languageOptionButton.Selected == 2 && !entry.Key.IsCSharp)   // .Net
            {
                entry.Key.Hide();
                continue;
            }

            // Is not on any and doesn't have the build
            if (_buildOptionButton.Selected != 0 && entry.Key.Build != BUILD_MAP[_buildOptionButton.Selected])
            {
                entry.Key.Hide();
                continue;
            }

            entry.Key.Show();
        }
    }

    private VersionEntryBase AddVersionEntry(bool isCard, string version, VersionData.BuildType build, bool isCSharp)
    {
        VersionEntryBase entry;
        if (isCard)
        {
            Card card = cardEntry.Instantiate<Card>();
            _cardGrid.AddChild(card);
            entry = card;
        }
        else
        {
            VersionListEntry versionListEntry = listEntry.Instantiate<VersionListEntry>();
            _listContainer.AddChild(versionListEntry);
            entry = versionListEntry;
        }

        entry.SetData(version, build, isCSharp);
        entry.DoubleClickButton.StateToggled += (bool state) => OnEntryToggled(state, entry);
        entry.DoubleClickButton.LaunchRequested += OnFolderOpenPressed;
        return entry;
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
        VersionData.ParsedVersionKey parts = VersionData.ParseKey(key);
        VersionEntryBase entry = AddVersionEntry(SettingsCache.Instance.GetData(VIEW_TAG), parts.version.ToString(), parts.build, parts.isCSharp);
        _versions.Add(entry, key);
    }

    private void OnOptionChanged(long index, string tag)
    {
        SettingsCache.Instance.AddOrUpdate(tag, new(index));
        Filter();
    }

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

    private void OnEntryToggled(bool state, VersionEntryBase entry)
    {
        _currentlySelected?.DoubleClickButton.ToggleOff();

        if (state)
            _currentlySelected = entry;
        else
            _currentlySelected = null;

        ToggleEntryButtons(!state);
    }

    private void OnLaunchPressed()
    {
        string key = _versions[_currentlySelected];
        string path = VersionCache.Instance.GetPath(key);
        OSAPI.RunGodotExe(path);
    }

    private void OnFolderOpenPressed()
    {
        string key = _versions[_currentlySelected];
        string path = VersionCache.Instance.GetPath(key);
        OSAPI.OpenFolder(path);
    }

    private void OnViewToggled(bool toggled)
    {
        SettingsCache.Instance.AddOrUpdate(VIEW_TAG, new(toggled));
        RefreshEntries();
    }
}
