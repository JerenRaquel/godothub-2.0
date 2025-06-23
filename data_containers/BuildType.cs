using System;

namespace DataContainer
{
    public struct BuildType(BuildType.FlagType flagType)
    {
        public enum FlagType { UNKNOWN, STABLE, RELEASE_CANDIDATE, BETA, DEV }

        public FlagType Type { get; set; } = flagType;
        public readonly string HTMLColor => Type switch
        {
            FlagType.STABLE => ColorTheme.Stable,
            FlagType.RELEASE_CANDIDATE => ColorTheme.ReleaseCandidate,
            FlagType.BETA => ColorTheme.Beta,
            FlagType.DEV => ColorTheme.Dev,
            _ => ColorTheme.Unknown
        };

        public override readonly string ToString()
        {
            return Type switch
            {
                FlagType.STABLE => "Stable",
                FlagType.RELEASE_CANDIDATE => "Release Candidate",
                FlagType.BETA => "Beta",
                FlagType.DEV => "Dev",
                _ => "Unknown"
            };
        }

        public static implicit operator string(BuildType buildType)
            => buildType.ToString();
        public static implicit operator BuildType(FlagType flagType)
            => new(flagType);

        public static explicit operator BuildType(string buildTypeStr)
        {
            return buildTypeStr switch
            {
                "Stable" => FlagType.STABLE,
                "Release Candidate" => FlagType.RELEASE_CANDIDATE,
                "Beta" => FlagType.BETA,
                "Dev" => FlagType.DEV,
                _ => FlagType.UNKNOWN
            };
        }

        // TODO: Check if this is needed -- I think its can be refactored out
        public static BuildType ParseBuildString(in string dirtyData)
        {
            string[] parts = dirtyData.Split(" [", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return new(FlagType.UNKNOWN);

            string newDataStr = parts[1];
            string[] newParts = newDataStr.Split("]", StringSplitOptions.RemoveEmptyEntries);
            string buildStr = newParts[0];
            return (BuildType)buildStr;
        }
    }
}