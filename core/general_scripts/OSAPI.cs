using System;
using System.IO;
using System.Text.RegularExpressions;
using DataContainer.DatabaseSys.Databases.ProjectDatabase;
using DataContainer.DatabaseSys.Databases.SettingDatabase;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using Godot;

public static partial class OSAPI
{
    public enum DeleteFlag
    {
        OK = 0,

        ERROR_PROJECT_NO_EXISTS = 0b001,
        ERROR_PROJECT_FAILED_DB_REMOVAL = 0b010,
        ERROR_PROJECT_FAILED_TRASH = 0b100,
        ERROR_PROJECT_ANY = ERROR_PROJECT_NO_EXISTS
            | ERROR_PROJECT_FAILED_DB_REMOVAL
            | ERROR_PROJECT_FAILED_TRASH,

        ERROR_SAVE_NO_EXISTS = 0b01_000,
        ERROR_SAVE_FAILED_TRASH = 0b10_000

    }

    public static string OS_USER_DATA_ROOT { get; private set; } = null;
    public static string DEFAULT_GODOT_USER_ROOT { get; private set; } = null;

    [GeneratedRegex(@"[^A-Za-z0-9-_ ]")]
    private static partial Regex MyRegex();
    private static Regex _regex = MyRegex();

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
        ProjectPathData? projectPathData = ProjectDatabase.Instance.GetPathData(in projectName);
        if (!projectPathData.HasValue) return -1; // Failed

        string projectPath = projectPathData.Value.ProjectGodotFileLocation;
        if (projectPath.Length == 0) return -1; // Failed

        long processID;
        if (withVerbose)
            processID = OS.CreateProcess(godotPath, ["--path", projectPath, "-e", "--verbose"]);
        else
            processID = OS.CreateProcess(godotPath, ["--path", projectPath, "-e"]);
        if (processID == -1) return -1; // Failed

        ProjectDatabase.Instance.UpdateTimeAccessed(projectName);
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
        ProjectPathData? projectPathData = ProjectDatabase.Instance.GetPathData(in projectName);
        if (!projectPathData.HasValue) return -1; // Failed

        string projectPath = projectPathData.Value.ProjectGodotFileLocation;
        if (projectPath.Length == 0) return -1; // Failed

        long processID;
        processID = OS.CreateProcess(godotPath, ["--path", projectPath]);
        if (processID == -1) return -1; // Failed

        return processID;
    }

    public static long RunTool(in TagKey toolKey, string projectName = "")
    {
        TagCommand commandData
            = TagDatabase.Instance.GetExecutableCommand(in toolKey, in projectName);

        long processID = OS.CreateProcess(commandData.Path, commandData.Args);
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

    public static DeleteFlag DeleteProject(string projectName, bool deleteSave)
    {
        //* Fetch the path data
        ProjectPathData? projectPathData = ProjectDatabase.Instance.GetPathData(in projectName);
        if (!projectPathData.HasValue) return DeleteFlag.ERROR_PROJECT_NO_EXISTS;

        //* Check if the directory exists...
        string rootFolder = projectPathData.Value.RootFolder;
        if (rootFolder == null || rootFolder.Length == 0)
            return DeleteFlag.ERROR_PROJECT_NO_EXISTS;
        if (!Directory.Exists(rootFolder))
            return DeleteFlag.ERROR_PROJECT_NO_EXISTS;

        //* Get saved path, if needed
        string projectUserFolder = null;
        if (deleteSave)
            projectUserFolder = ProjectDatabase.Instance.GetProjectUserDirectory(in projectName);

        //* Attempt to remove project from Database
        if (!ProjectDatabase.Instance.DeleteProject(in projectName))
            return DeleteFlag.ERROR_PROJECT_FAILED_DB_REMOVAL;

        //* Attempt to Trash Project Directory
        if (OS.MoveToTrash(rootFolder) != Error.Ok)
            return DeleteFlag.ERROR_PROJECT_FAILED_TRASH;

        //* If not deleting the save folder, stop here
        if (!deleteSave) return DeleteFlag.OK;

        //* Check if the save folder exists
        if (!Directory.Exists(projectUserFolder))
            return DeleteFlag.ERROR_SAVE_NO_EXISTS;

        //* Attempt to delete save folder
        if (OS.MoveToTrash(projectUserFolder) != Error.Ok)
            return DeleteFlag.ERROR_SAVE_FAILED_TRASH;

        return DeleteFlag.OK;
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

    public static string FormatFolderName(string rawName)
    {
        int idx = SettingsDatabase.Instance.GetData(SettingsDatabase.PROJECT_NAMING);
        string folderName = idx switch
        {
            0 => rawName.Replace("-", " ").Replace("_", " ").ToPascalCase(),    // PascalCase
            1 => rawName.Replace("-", " ").ToSnakeCase(),                       // snake_case
            2 => rawName.ToSnakeCase().Replace("_", "-"),                       // kebab-case
            3 => rawName.Replace("-", " ").Replace("_", " ").ToCamelCase(),     // camelCase
            _ => rawName
        };

        if (IsValidFolderName(folderName)) return folderName;
        return null;
    }

    //? Might want to come back to this to make it less naive
    public static string SanitizePath(in string rawPath)
    {
        return rawPath.Replace("\\", "/");
    }
}