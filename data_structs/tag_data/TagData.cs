using System.Collections.Generic;

public partial class TagData
{
    // { tag : colorCode }  -- Formatted with both
    private Dictionary<string, Data> _softwareTags = [];
    private Dictionary<string, Data> _projectTags = [];

    public void AddOrUpdate(bool isSoftware, string name, Data data)
    {
        if (isSoftware)
            _softwareTags.Add(name, data);
        else
            _projectTags.Add(name, data);
    }

    public string GetColor(bool isSoftware, string name, string defaultValue = "000000")
    {
        Data data = GetData(isSoftware, name);
        if (data.IsNull) return defaultValue;
        return data.ColorCode;
    }

    public string GetPath(string name, string defaultValue = "")
    {
        if (!_softwareTags.TryGetValue(name, out Data value)) return defaultValue;
        return value.Path;
    }

    public string GenerateCommandString(string name)
    {
        Data data = GetData(true, name);
        if (data.IsNull) return null;

        return data.Command;
    }

    public void Overwrite(TagData other)
    {
        _softwareTags = [];
        _projectTags = [];

        foreach (KeyValuePair<string, Data> entry in other._softwareTags)
            _softwareTags.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, Data> entry in other._projectTags)
            _projectTags.Add(entry.Key, entry.Value);
    }

    private Data GetData(bool isSoftware, string name)
    {
        if (isSoftware)
        {
            if (!_softwareTags.TryGetValue(name, out Data value)) return new();
            return value;
        }
        else
        {
            if (!_projectTags.TryGetValue(name, out Data value)) return new();
            return value;
        }
    }
}