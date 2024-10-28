public partial class VersionData
{
    public enum BuildType { STABLE, RELEASE_CANDIDATE, BETA, DEV }

    public readonly struct VersionObject
    {
        private readonly Version _version = new();
        private readonly bool _isCSharp = false;
        private readonly BuildType _buildType;
        private readonly string _path;
        private readonly string _key;

        public string Key => _key;
        public string Path => _path;
        public readonly string Version => _version.ToString();
        public readonly bool IsCSharp => _isCSharp;
        public readonly string Build => BuildEnumToString();

        public VersionObject(VersionObject verObj)
        {
            this = new VersionObject(
                verObj._version,
                verObj._isCSharp,
                verObj._buildType,
                verObj._path
            );
        }

        public VersionObject(Version version, bool isCSharp, BuildType type, string path)
        {
            _version = version;
            _isCSharp = isCSharp;
            _buildType = type;
            _path = path;
            _key = GenerateKey();
        }

        private string GenerateKey()
        {
            string type = "STD";
            if (_isCSharp) type = "DOTNET";

            return $"{_version.ToString()}_{type}_{BuildEnumToString()}";
        }

        private string BuildEnumToString()
        {
            return _buildType switch
            {
                BuildType.STABLE => "Stable",
                BuildType.RELEASE_CANDIDATE => "Release Candidate",
                BuildType.BETA => "Beta",
                BuildType.DEV => "Dev",
                _ => "Unknown"
            };
        }
    }

    public readonly struct ParsedVersionKey
    {
        public readonly bool isValid;
        public readonly string version;
        public readonly bool isCSharp;
        public readonly string build;

        public ParsedVersionKey() => isValid = false;

        public ParsedVersionKey(VersionObject obj)
        {
            version = obj.Version;
            isCSharp = obj.IsCSharp;
            build = obj.Build;
        }
    }
}