using Godot;
using System;

public partial class VersionEntryBase : MarginContainer
{
    protected string _versionStr;

    public DoubleClickButton DoubleClickButton { get; private set; }
    public bool IsCSharp { get; protected set; } = false;
    public VersionData.BuildType Build { get; protected set; }

    public override void _Ready() => DoubleClickButton = GetNode<DoubleClickButton>("%DoubleClickButton");

    public virtual void SetData(string version, VersionData.BuildType build, bool isCSharp) { }
}
