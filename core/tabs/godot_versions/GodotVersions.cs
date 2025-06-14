using DataContainer.DatabaseSys.Databases.SettingDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;
using System.Collections.Generic;

public partial class GodotVersions : TabBase
{
    //? Should these be in the SettingsTag struct?
    public readonly SettingsTag VIEW_KEY = new("GLOBAL/GodotVersion/view_mode/BOOL");
    public readonly SettingsTag LANGUAGE_KEY = new("GLOBAL/GodotVersion/lang_support_mode/LONG");
    public readonly SettingsTag RELEASE_KEY = new("GLOBAL/GodotVersion/release_mode/LONG");

    private readonly VersionDatabase.BuildType[] BUILD_MAP = [
        VersionDatabase.BuildType.UNKNOWN,
        VersionDatabase.BuildType.DEV,
        VersionDatabase.BuildType.BETA,
        VersionDatabase.BuildType.RELEASE_CANDIDATE,
        VersionDatabase.BuildType.STABLE
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

    private Dictionary<VersionEntryBase, VersionKey> _versions = [];
    private VersionEntryBase _currentlySelected = null;

    public override void _ExitTree()
    {
        SettingsDatabase.Instance.AddOrUpdate(VIEW_KEY, _viewCheckButton.ButtonPressed);
        SettingsDatabase.Instance.AddOrUpdate(LANGUAGE_KEY, _languageOptionButton.Selected);
        SettingsDatabase.Instance.AddOrUpdate(RELEASE_KEY, _buildOptionButton.Selected);
    }

    public override void _Ready()
    {
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnLocatePressed;
        _openFolderButton = GetNode<Button>("%OpenLocationButton");
        _openFolderButton.Pressed += OnFolderOpenPressed;
        _runButton = GetNode<Button>("%RunButton");
        _runButton.Pressed += OnLaunchPressed;
        _languageOptionButton = GetNode<OptionButton>("%LangOptionButton");
        _languageOptionButton.ItemSelected += OnOptionLanguageChanged;
        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _buildOptionButton.ItemSelected += OnOptionReleaseChanged;
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
        _viewCheckButton.SetPressedNoSignal(SettingsDatabase.Instance.GetDataOrSetDefault(VIEW_KEY, new(true)));
        _languageOptionButton.Selected = SettingsDatabase.Instance.GetDataOrSetDefault(LANGUAGE_KEY, new(0));
        _buildOptionButton.Selected = SettingsDatabase.Instance.GetDataOrSetDefault(RELEASE_KEY, new(0));

        RefreshEntries();
    }

    private void RefreshEntries()
    {
        foreach (KeyValuePair<VersionEntryBase, VersionKey> entry in _versions)
        {
            if (entry.Key.IsQueuedForDeletion()) continue;

            entry.Key.QueueFree();
        }
        _versions = [];
        _currentlySelected?.DoubleClickButton.ToggleOff();
        _currentlySelected = null;

        if (SettingsDatabase.Instance.GetData(VIEW_KEY))
        {
            _listView.Hide();
            _cardView.Show();
        }
        else
        {
            _listView.Show();
            _cardView.Hide();
        }

        foreach (VersionKey key in VersionDatabase.Instance.SortedKeys)
            OnVersionLocated((string)key);
        Filter();
    }

    private void Filter()
    {
        foreach (KeyValuePair<VersionEntryBase, VersionKey> entry in _versions)
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

    private VersionEntryBase AddVersionEntry(bool isCard, in VersionKey versionKey)
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

        entry.SetData(in versionKey);
        entry.DoubleClickButton.StateToggled += state => OnEntryToggled(state, entry);
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
        VersionKey versionKey = new(key);
        VersionEntryBase entry = AddVersionEntry(SettingsDatabase.Instance.GetData(VIEW_KEY), in versionKey);
        _versions.Add(entry, versionKey);
    }

    private void OnOptionLanguageChanged(long index)
    {
        SettingsDatabase.Instance.AddOrUpdate(in LANGUAGE_KEY, new(index));
        Filter();
    }

    private void OnOptionReleaseChanged(long index)
    {
        SettingsDatabase.Instance.AddOrUpdate(in RELEASE_KEY, new(index));
        Filter();
    }

    private void OnDeletePressed()
    {
        VersionKey key = _versions[_currentlySelected];
        if (!VersionDatabase.Instance.RemoveVersion(key)) return;

        // Success
        _versions.Remove(_currentlySelected);
        _currentlySelected.QueueFree();
        _currentlySelected = null;
        ToggleEntryButtons(true);
    }

    private void OnEntryToggled(bool state, VersionEntryBase entry)
    {
        _currentlySelected?.DoubleClickButton.ToggleOff();
        _currentlySelected = state ? entry : null;
        ToggleEntryButtons(!state);
    }

    private void OnLaunchPressed()
    {
        string path = VersionDatabase.Instance.GetPath(_versions[_currentlySelected]);
        OSAPI.RunGodotExe(path);
    }

    private void OnFolderOpenPressed()
    {
        string path = VersionDatabase.Instance.GetPath(_versions[_currentlySelected]);
        OSAPI.OpenFolder(path);
    }

    private void OnViewToggled(bool toggled)
    {
        SettingsDatabase.Instance.AddOrUpdate(VIEW_KEY, new(toggled));
        RefreshEntries();
    }
}
