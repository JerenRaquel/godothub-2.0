using Godot;

public static partial class OSAPI
{
    private static string _defaultUserFolderPath = null;

    public static bool OpenFolder(string path)
    {
        if (path == null || path.Length == 0) return false;
        return OS.ShellShowInFileManager(path) == Error.Ok;
    }

    public static bool OpenUserFolder(string projectName)
    {
        // TODO: Check to see if the project has a different user path
        return OpenFolder(GetDefaultUserPath() + projectName);
    }

    public static long OpenGodotProject(string godotPath, string projectName, bool withVerbose = false)
    {
        string projectPath = ProjectCache.Instance.GetProjectPath(projectName);
        if (projectPath.Length == 0) return -1; // Failed

        long processID;
        if (withVerbose)
            processID = OS.CreateProcess(godotPath, ["--path", projectPath, "-e", "--verbose"]);
        else
            processID = OS.CreateProcess(godotPath, ["--path", projectPath, "-e"]);
        if (processID == -1) return -1; // Failed

        ProjectCache.Instance.UpdateTimeAccessed(projectName);
        return processID;
    }

    public static long RunGodotExe(string godotPath, bool withVerbose = false)
    {
        long processID;
        if (withVerbose)
            processID = OS.CreateProcess(godotPath, ["-p", "--verbose"]);
        else
            processID = OS.CreateProcess(godotPath, ["-p"]);
        if (processID == -1) return -1; // Failed

        return processID;
    }

    public static long RunTool(string toolName, string projectName = "")
    {
        TagData.CommandParts executableCommand = TagCache.Instance.GetExecutableCommand(toolName, projectName);

        long processID = OS.CreateProcess(executableCommand.Command, executableCommand.Args);
        if (processID == -1) return -1; // Failed

        return processID;
    }

    private static string GetDefaultUserPath()
    {
        if (_defaultUserFolderPath != null) return _defaultUserFolderPath;

        string appUserPath = ProjectSettings.GlobalizePath("user://");
        appUserPath = appUserPath.Replace((string)ProjectSettings.GetSetting("application/config/name") + "/", "");
        _defaultUserFolderPath ??= appUserPath;

        return _defaultUserFolderPath;
    }
}