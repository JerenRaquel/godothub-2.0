using DataContainer;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;

public partial class VersionEntryBase : MarginContainer
{
    protected string _versionStr;

    public DoubleClickButton DoubleClickButton { get; private set; }
    public bool IsCSharp { get; protected set; } = false;
    public BuildType Build { get; protected set; }

    public override void _Ready() => DoubleClickButton = GetNode<DoubleClickButton>("%DoubleClickButton");

    public virtual void SetData(in VersionKey key) { }
}
