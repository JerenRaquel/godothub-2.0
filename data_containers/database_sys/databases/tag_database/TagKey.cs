namespace DataContainer.DatabaseSys.Databases.TagDatabase
{
    public readonly struct TagKey(string tagName, bool isSoftware)
    {
        public readonly string TagName = tagName;
        public readonly bool IsSoftware = isSoftware;
        private readonly string key = $"{tagName}_{(isSoftware ? "software" : "project")}";

        public readonly string Key => key;

        public override string ToString() => key;

        public static TagKey? ParseRawKey(string rawKey)
        {
            string[] parts = rawKey.Split('_', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;
            return new(parts[0], parts[1] == "software");
        }
    }
}