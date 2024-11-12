using System.Collections.Generic;

public partial class TemplateData
{
    private Dictionary<string, DataNode> _roots = [];

    public Dictionary<string, DataNode>.Enumerator RawTemplateData => _roots.GetEnumerator();
    public int TemplateCount => _roots.Count;

    public TemplateData() { }
    public TemplateData(TemplateData other) => OverwriteWith(other);

    public DataNode AddTemplate(string templateName)
    {
        if (_roots.ContainsKey(templateName)) return new();

        DataNode node = new(templateName);
        _roots.Add(templateName, node);
        return node;
    }

    public void OverwriteWith(TemplateData other)
    {
        _roots.Clear();
        foreach (KeyValuePair<string, DataNode> entry in other._roots)
            _roots.Add(entry.Key, entry.Value);
    }
}