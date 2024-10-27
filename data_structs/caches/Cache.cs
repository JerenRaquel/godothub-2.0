public abstract class Cache(string saveLocation)
{
    protected readonly string SAVE_LOCATION = saveLocation;
    protected static readonly object padlock = new();

    protected bool _isDirty = false;

    public abstract bool LoadData();
    public abstract void WriteData();
    public abstract void ForceWrite();
}