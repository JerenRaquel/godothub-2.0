using System;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

public static partial class OSAPI
{
    public static string OS_USER_DATA_ROOT { get; private set; } = null;
    public static string DEFAULT_GODOT_USER_ROOT { get; private set; } = null;

    private static Regex _regex = new(@"[^A-Za-z0-9-_ ]");

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

    public static bool IsDirectoryEmpty(string path)
        => Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;

    public static bool IsValidFolderName(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return false;
        if (char.IsNumber(folderName[0])) return false;
        return _regex.Count(folderName) == 0;
    }

    public static Tuple<bool, bool> DeleteProject(string projectName, bool deleteSave)
    {
        string projectFolder = ProjectCache.Instance.GetProjectFolder(projectName);
        if (projectFolder == null) return new(false, false);

        bool state = OS.MoveToTrash(projectFolder) == Error.Ok;
        if (!state) return new(false, false);

        if (deleteSave)
        {
            string projectSaveFolder = ProjectCache.Instance.GetProjectSaveFolder(projectName);
            if (projectSaveFolder == null)
            {
                ProjectCache.Instance.DeleteProject(projectName);
                return new(true, false);
            }

            bool saveState = OS.MoveToTrash(projectSaveFolder) == Error.Ok;
            ProjectCache.Instance.DeleteProject(projectName);
            return new(true, saveState);
        }
        ProjectCache.Instance.DeleteProject(projectName);
        return new(true, true);
    }

    public static bool CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return false;
        }
        return true;
    }
}