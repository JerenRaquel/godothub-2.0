using Godot;

public partial class ProjectData
{
    public readonly struct Version
    {
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

        public readonly int Major { get; }
        public readonly int Minor { get; }
        public readonly bool isValid;

        public Vector2I ToGDVector() => new Vector2I(Major, Minor);

        public override string ToString()
        {
            if (!isValid) return "Unknown";
            return $"{Major}.{Minor}";
        }

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

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}