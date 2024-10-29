using Godot;
using System;
using System.Collections.Generic;

public partial class GodotVersions : PanelContainer
{
    [Export] private PackedScene card;

    private Button _locateButton;
    private GridContainer _cardGrid;
    private LocateGodotWindow _locateWindow;

    private Dictionary<Control, string> _versions = [];

    public override void _ExitTree()
    {
        _locateButton.Pressed -= OnLocatePressed;
    }

    public override void _Ready()
    {
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnLocatePressed;
        _cardGrid = GetNode<GridContainer>("%CardViewContainer");
        _locateWindow = GetNode<LocateGodotWindow>("%LocateGodotWindow");
        _locateWindow.VersionLocated += OnVersionLocated;
    }

    private Card AddCard(string version, string build, bool isCSharp)
    {
        Card cardInstance = card.Instantiate<Card>();
        _cardGrid.AddChild(cardInstance);
        cardInstance.SetData(version, build, isCSharp);
        return cardInstance;
    }

    private void OnLocatePressed() => _locateWindow.Show();

    private void OnVersionLocated(string key)
    {
        // TODO: Determine which version to spawn based on display

        // TEMP: Remove once TODO^ is compete
        VersionData.ParsedVersionKey parts = VersionData.ParseKey(key);
        Control card = AddCard(parts.version.ToString(), VersionData.BuildEnumToString(parts.build), parts.isCSharp);
        _versions.Add(card, key);
    }
}
