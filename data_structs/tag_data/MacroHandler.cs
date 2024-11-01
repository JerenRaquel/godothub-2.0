using System.Collections.Generic;

public partial class MacroHandler
{
    #region Singleton Instance
    private static MacroHandler _instance;
    private static readonly object padlock = new();

    public static MacroHandler Instance => _instance;

    private MacroHandler() { }

    public static MacroHandler Initialize()
    {
        lock (padlock)
        {
            _instance ??= new();
            return _instance;
        }
    }
    #endregion

    public static readonly string[] PROJECT_MACROS = [
        "USER_FOLDER",
        "ROOT_FOLDER",
        "PROJECT_FOLDER",
        "PROJECT_NAME"
    ];

    public static readonly string[] GENERAL_MACROS = [

    ];

    public struct CommandParts
    {
        private bool _isNull = true;

        public string Command { get; private set; }
        public string[] Args { get; private set; }
        public readonly bool IsNull => _isNull;

        public CommandParts() { }
        public CommandParts(string command, string[] args)
        {
            Command = command;
            Args = args;
            _isNull = false;
        }
    }

    public static CommandParts GenerateCommand(string toolName, string projectName)
    {
        string command = TagCache.Instance.GetExecutableCommand(toolName);
        if (command == null) return new();

        // string userDirectory = 
        string projectDirectory = ProjectCache.Instance.GetProjectPath(projectName);
        string rootDirectory = ProjectCache.Instance.GetProjectFolder(projectName);

        string[] parts = command.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
        string[] results = new string[parts.Length - 1];
        for (int i = 1; i < parts.Length; i++)
        {
            // Not a MACRO
            if (!parts[i].StartsWith('{') && !parts[i].EndsWith('}'))
            {
                results[i - 1] = parts[i];
                continue;
            }

            //* GENERAL MACROS


            // If project name is null, dont replace any project based MACROS
            if (projectName == null)
            {
                results[i - 1] = parts[i];
                continue;
            }

            //* Project Based MACROS
            if (parts[i] == PROJECT_MACROS[0])
                // parts[i] = USER_DIRECTORY;
                parts[i] = "";   // TODO: Plug this in once implemented
            else if (parts[i] == PROJECT_MACROS[1])
                parts[i] = rootDirectory;
            else if (parts[i] == PROJECT_MACROS[2])
                parts[i] = projectDirectory;
            else if (parts[i] == PROJECT_MACROS[3])
                parts[i] = projectName;
            else
            {
                results[i - 1] = parts[i];
                continue;
            }
        }
        return new(parts[0], results);
    }

}