public static class ColorTheme
{
    public static readonly string BaseBlue = "478cbf";
    public static readonly string HighlightBlue = "3dbde0";
    public static readonly string Unknown = "ffff70";
    public static readonly string Stable = "6aff7c";
    public static readonly string ReleaseCandidate = "41a6b5";
    public static readonly string Beta = "24acf2";
    public static readonly string Dev = "9d5277";
    public static readonly string Compat = "24acf2";
    public static readonly string Mobile = "9d5277";
    public static readonly string Forward = "6aff7c";

    public static string BBCodeColor(this string str, string colorCode) => $"[color='{colorCode}']{str}[/color]";

    public static string GetColorFromBuild(VersionData.BuildType build)
    {
        return build switch
        {
            VersionData.BuildType.STABLE => Stable,
            VersionData.BuildType.RELEASE_CANDIDATE => ReleaseCandidate,
            VersionData.BuildType.BETA => Beta,
            VersionData.BuildType.DEV => Dev,
            _ => Unknown
        };
    }
}