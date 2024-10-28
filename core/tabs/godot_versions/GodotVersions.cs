using Godot;
using System;

public partial class GodotVersions : PanelContainer
{
    [Export] private PackedScene card;

    private Button _locateButton;
    private GridContainer _cardGrid;
    private LocateGodotWindow _locateWindow;
    private FileDialog _fileDialog;

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
        _fileDialog = GetNode<FileDialog>("%FileDialog");

        _fileDialog.Hide();
    }

    private void AddCard(string version, string build, bool isCSharp)
    {
        Card cardInstance = card.Instantiate<Card>();
        _cardGrid.AddChild(cardInstance);
        cardInstance.SetData(version, build, isCSharp);
    }

    private void OnLocatePressed() => _locateWindow.Show();
}
