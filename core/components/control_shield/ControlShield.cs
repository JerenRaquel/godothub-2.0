using Godot;
using System;

public partial class ControlShield : Panel
{
    #region Singleton Instance
    private static ControlShield _instance;

    public static ControlShield Instance => _instance;

    private ControlShield() => Hide();
    #endregion

    public override void _Ready() => _instance = this;
}
