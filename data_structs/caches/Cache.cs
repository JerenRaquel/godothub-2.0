public abstract class Cache
{
    protected bool _isDirty = false;

    public abstract bool LoadData();
    public abstract void WriteData();
    public abstract void ForceWrite();
}