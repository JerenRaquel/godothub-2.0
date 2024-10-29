using Godot;

public static partial class OSAPI
{
    public static bool OpenFolder(string path) => OS.ShellShowInFileManager(path) == Error.Ok;

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

        // TODO: Update Last Edited

        // TODO: Close HUB is setting is enabled

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
}