using Godot;
using System;

public partial class GodotVersions : PanelContainer
{
    [Export] private PackedScene card;

    private GridContainer _cardGrid;

    public override void _Ready()
    {
        _cardGrid = GetNode<GridContainer>("%CardViewContainer");

        AddCard("4.5", "Dev", false);
        AddCard("5.4", "Beta", true);
    }

    private void AddCard(string version, string build, bool isCSharp)
    {
        Card cardInstance = card.Instantiate<Card>();
        _cardGrid.AddChild(cardInstance);
        cardInstance.SetData(version, build, isCSharp);
    }
}
