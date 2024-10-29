using System;

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

    public Version(string str)
    {
        string[] parts = str.Split(".", System.StringSplitOptions.RemoveEmptyEntries);
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
        catch (System.Exception)
        {
            isValid = false;
        }
    }

    public readonly int Major { get; }
    public readonly int Minor { get; }
    public readonly bool isValid;

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