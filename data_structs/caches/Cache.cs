public abstract class Cache
{
    private bool _isDirty = false;

    public abstract bool LoadData();
    public abstract bool WriteData();
    public abstract void ForceWrite();

    private void _ParseJsonData()
    {

    }
}