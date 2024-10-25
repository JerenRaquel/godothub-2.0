using System.Collections.Generic;
using Godot;

public partial class ProjectCache : Cache
{
    private Dictionary<string, ProjectDataState> _projects;

    public override bool LoadData()
    {
        return false;
    }

    public override bool WriteData()
    {
        // Check if dirty, no point to write if not
        if (!this._isDirty) { return true; }

        // If there's no projects, no point to write
        if (this._projects.Count == 0) { return true; }

        foreach (KeyValuePair<string, ProjectDataState> entry in this._projects)
        {
            // Write
        }

        return true;
    }

    public override void ForceWrite()
    {
    }
}