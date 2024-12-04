using System.IO;
using Godot;

public static partial class OSAPI
{
    public static string OS_USER_DATA_ROOT { get; private set; } = null;
    public static string DEFAULT_GODOT_USER_ROOT { get; private set; } = null;

    public static void Initialize()
    {
        if (OS_USER_DATA_ROOT != null) return;

        OS_USER_DATA_ROOT = OS.GetDataDir();
        DEFAULT_GODOT_USER_ROOT = OS_USER_DATA_ROOT + "/Godot/app_userdata/";
    }

    public static bool OpenFolder(string path)
    {
        if (path == null || path.Length == 0) return false;
        return OS.ShellShowInFileManager(path) == Error.Ok;
    }

    public static bool OpenUserFolder(string projectName)
    {
        string path = DEFAULT_GODOT_USER_ROOT + projectName;
        if (!Directory.Exists(path))
            path = OS_USER_DATA_ROOT + "/" + projectName;

        if (!Directory.Exists(path))
        {
            NotifcationManager.Instance.NotifyError($"Could not locate save data for project: {projectName}");
            return false;
        }

        return OpenFolder(path);
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

    public static long RunGodotProject(string godotPath, string projectName)
    {
        string projectPath = ProjectCache.Instance.GetProjectPath(projectName);
        if (projectPath.Length == 0) return -1; // Failed

        long processID;
        processID = OS.CreateProcess(godotPath, ["--path", projectPath]);
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
}