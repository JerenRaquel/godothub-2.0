public partial class ProjectCache : Cache
{
    public override bool LoadData()
    {
        return false;
    }

    public override bool WriteData()
    {
        return false;
    }

    public override void ForceWrite()
    {
    }
}