using System;

public partial class VersionData
{
    public enum BuildType { UNKNOWN, STABLE, RELEASE_CANDIDATE, BETA, DEV }

    public readonly struct ParsedVersionKey
    {
        public readonly bool isValid;
        public readonly Version version;
        public readonly bool isCSharp;
        public readonly BuildType build;

        public ParsedVersionKey() => isValid = false;

        public ParsedVersionKey(string key)
        {
            string[] data = key.Split("_", System.StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 3)
            {
                isValid = false;
                return;
            }

            version = new(data[0]);
            isCSharp = data[1] == "DOTNET";
            build = StringToBuildEnum(data[2]);
            isValid = true;
        }
    }

    public static string GenerateKey(Version version, bool isCSharp, BuildType type)
        => GenerateKeys(version, isCSharp, type).Item1;

    public static string GeneratePartialKey(Version version, bool isCSharp)
    {
        string typeStr = "STD";
        if (isCSharp) typeStr = "DOTNET";
        return $"{version}_{typeStr}";
    }

    public static ParsedVersionKey ParseKey(string key) => new(key);

    public static string BuildEnumToString(BuildType type)
    {
        return type switch
        {
            BuildType.STABLE => "Stable",
            BuildType.RELEASE_CANDIDATE => "Release Candidate",
            BuildType.BETA => "Beta",
            BuildType.DEV => "Dev",
            _ => "Unknown"
        };
    }

    public static BuildType StringToBuildEnum(string str)
    {
        return str switch
        {
            "Stable" => BuildType.STABLE,
            "Release Candidate" => BuildType.RELEASE_CANDIDATE,
            "Beta" => BuildType.BETA,
            "Dev" => BuildType.DEV,
            _ => BuildType.UNKNOWN
        };
    }

    private static Tuple<string, string> GenerateKeys(Version version, bool isCSharp, BuildType type)
    {
        string partialKey = GeneratePartialKey(version, isCSharp);
        return new(
            $"{partialKey}_{BuildEnumToString(type)}",
            partialKey
        );
    }
}