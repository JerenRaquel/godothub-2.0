using Godot;
using System;

public partial class LocateGodotWindow : WindowBase
{
    [Export] private FileDialog _fileDialog;

    public override void _Ready()
    {
        base._Ready();

        DisplayError("Missing Data");
    }
}
