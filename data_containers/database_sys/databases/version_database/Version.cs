using System;

namespace DataContainer.DatabaseSys.Databases.VersionDatabase
{
    public readonly struct Version
    {
        public readonly int Major { get; }
        public readonly int Minor { get; }
        public readonly bool isValid;

        public Version()
        {
            Major = -1;
            Minor = -1;
            isValid = false;
        }

        public Version(int major, int minor)
        {
            Major = major;
            Minor = minor;
            isValid = true;
        }

        public Version(Version copy)
        {
            Major = copy.Major;
            Minor = copy.Minor;
            isValid = true;
        }

        public Version(string str)
        {
            string[] parts = str.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                isValid = false;
                return;
            }

            try
            {
                Major = Int32.Parse(parts[0]);
                Minor = Int32.Parse(parts[1]);
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }
        }

        public override string ToString()
        {
            if (!isValid) return "Unknown";
            return $"{Major}.{Minor}";
        }

        public static explicit operator string(Version version) => version.ToString();

        public static bool operator ==(Version x, Version y)
        {
            return x.Major == y.Major && x.Minor == y.Minor;
        }

        public static bool operator !=(Version x, Version y)
        {
            return x.Major != y.Major && x.Minor != y.Minor;
        }

        public override readonly bool Equals(object obj)
        {
            if (!(obj is Version)) return false;

            return Major == ((Version)obj).Major && Minor == ((Version)obj).Minor;
        }

        public override readonly int GetHashCode() => base.GetHashCode();

        public static string ParseVersionStr(string dataStr)
        {
            string[] parts = dataStr.Split(" [", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            return parts[0].Replace("v", "");
        }

        public static Version ParseVersionStr(in string rawDataStr)
        {
            // "v{key.version}" : "{key.buildType}]"
            string[] parts = rawDataStr.Split(" [", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return new();

            // Major.Minor
            string rawVersion = parts[0].Replace("v", "");
            return new(rawVersion);
        }
    }
}