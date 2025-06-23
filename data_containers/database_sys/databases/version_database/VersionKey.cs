namespace DataContainer.DatabaseSys.Databases.VersionDatabase
{
    public class VersionKey
    {
        public Version version;
        public bool isDotNet;
        public BuildType buildType;

        public string PartialKey { get; private set; }
        public string FullKey { get; private set; }
        public bool IsValid { get; private set; } = true;
        public bool IsFull => buildType.Type != BuildType.FlagType.UNKNOWN;
        public string VersionStr => (string)version;

        public VersionKey() => IsValid = false;

        public VersionKey(string rawKey)
        {
            string[] data = rawKey.Split("_", System.StringSplitOptions.RemoveEmptyEntries);
            // Check if key is invalid
            if (data.Length < 2)
            {
                IsValid = true;
                return;
            }
            version = new(data[0]);
            isDotNet = data[1] == "DOTNET";
            // Check if we can create a partial key
            if (data.Length == 2)
            {
                buildType = BuildType.FlagType.UNKNOWN;
                GenerateKeys();
                return;
            }

            // Create the full key + partial
            buildType = (BuildType)data[2];

            GenerateKeys();
        }

        public VersionKey(Version version, bool isDotNet)
        {
            this.version = version;
            this.isDotNet = isDotNet;
            buildType = BuildType.FlagType.UNKNOWN;
            GenerateKeys();
        }

        public VersionKey(Version version, bool isDotNet, BuildType type)
        {
            this.version = version;
            this.isDotNet = isDotNet;
            buildType = type;
            GenerateKeys();
        }

        public static explicit operator string(VersionKey key) => key.FullKey;

        public static explicit operator VersionKey(string rawKey) => new(rawKey);

        public static bool operator ==(VersionKey x, VersionKey y) => x.FullKey == y.FullKey;

        public static bool operator !=(VersionKey x, VersionKey y) => x.FullKey != y.FullKey;

        public override bool Equals(object obj)
        {
            if (!(obj is VersionKey)) return false;

            VersionKey other = (VersionKey)obj;
            return FullKey == other.FullKey;
        }

        public override int GetHashCode() => FullKey.GetHashCode();

        private void GenerateKeys()
        {
            PartialKey = GeneratePartialKey(version, isDotNet);
            FullKey = $"{PartialKey}_{buildType}";
        }

        public static string GeneratePartialKey(Version version, bool isDotNet)
        {
            string typeStr = "STD";
            if (isDotNet) typeStr = "DOTNET";

            return $"{version}_{typeStr}";
        }
    }
}