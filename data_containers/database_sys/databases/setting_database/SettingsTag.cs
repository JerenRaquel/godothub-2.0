namespace DataContainer.DatabaseSys.Databases.SettingDatabase
{
    public struct SettingsTag
    {
        public string Group { get; private set; }
        public string Section { get; private set; }
        public string Tag { get; private set; }
        public SettingsData.Type Type { get; private set; }

        public readonly string Key => $"{Group}/{Section}/{Tag}/{TypeToStr(Type)}";
        public readonly string TypeStr => TypeToStr(Type);

        public SettingsTag(string rawStr)
        {
            string[] parts = rawStr.Split("/", System.StringSplitOptions.RemoveEmptyEntries);
            Group = parts[0];
            Section = parts[1];
            Tag = parts[2];
            Type = StrToType(parts[3]);
        }

        public SettingsTag(string group, string section, string tag, SettingsData.Type type)
        {
            Group = group;
            Section = section;
            Tag = tag;
            Type = type;
        }

        public override readonly string ToString() => Key;

        public static bool operator ==(SettingsTag x, SettingsTag y) => x.Key == y.Key;

        public static bool operator !=(SettingsTag x, SettingsTag y) => x.Key != y.Key;

        public override readonly bool Equals(object obj)
        {
            if (!(obj is SettingsTag)) return false;

            SettingsTag other = (SettingsTag)obj;
            return Key == other.Key;
        }

        public override readonly int GetHashCode() => Key.GetHashCode();

        public static string TypeToStr(SettingsData.Type type)
        {
            return type switch
            {
                SettingsData.Type.BOOL => "BOOL",
                SettingsData.Type.LONG => "LONG",
                SettingsData.Type.STRING_LIST => "STRING_LIST",
                _ => "NULL"
            };
        }

        public static SettingsData.Type StrToType(string typeStr)
        {
            return typeStr switch
            {
                "BOOL" => SettingsData.Type.BOOL,
                "LONG" => SettingsData.Type.LONG,
                "STRING_LIST" => SettingsData.Type.STRING_LIST,
                _ => SettingsData.Type.NULL
            };
        }
    }
}