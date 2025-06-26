using DataContainer.DatabaseSys.Databases.TagDatabase;

namespace DataContainer.DatabaseSys.Databases.TemplateDatabase
{
    public partial class TemplateDatabase : Database<string, TemplateData>
    {
        public string[] TemplateNames => [.. _data.Keys];

        public TagKey[] GetTags(in string templateName,
            TagDatabase.TagDatabase.TagFlag flag)
        {
            if (!_data.ContainsKey(templateName)) return [];
            return _data[templateName].GetTags(flag);
        }

        public bool Generate(in string templateName, in string path)
        {
            if (!_data.TryGetValue(templateName, out TemplateData template))
                return false;
            return template.Generate(in path);
        }
    }
}