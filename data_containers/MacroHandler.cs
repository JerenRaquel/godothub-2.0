using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.ProjectDatabase;
using DataContainer.DatabaseSys.Databases.TagDatabase;

namespace DataContainer
{
    public abstract class MacroHandler
    {
        public static readonly string[] PROJECT_MACROS = [
            "USER_FOLDER",
            "ROOT_FOLDER",
            "PROJECT_FOLDER",
            "PROJECT_NAME"
        ];

        public static readonly string[] GENERAL_MACROS = [
            "TIME"
        ];

        public static TagCommand SubsituteMacros(in TagCommand commandData, in string projectName)
        {
            if (commandData == null) return null;

            string command = commandData.Command;
            string userDirectory = "";
            string projectDirectory = "";
            string rootDirectory = "";

            if (projectName != null)
            {
                userDirectory = ProjectDatabase.Instance.GetProjectUserDirectory(in projectName);
                ProjectPathData? pathData = ProjectDatabase.Instance.GetPathData(in projectName);
                if (pathData.HasValue)
                {
                    projectDirectory = pathData.Value.ProjectGodotFileLocation;
                    rootDirectory = pathData.Value.RootFolder;
                }
            }

            string[] parts = commandData.Args;
            List<string> results = [];
            for (int i = 0; i < parts.Length; i++)
            {
                // Not a MACRO
                if (!parts[i].StartsWith('{') && !parts[i].EndsWith('}'))
                {
                    results.Add(parts[i]);
                    continue;
                }

                //* GENERAL MACROS

                // If project name is null, skip project based MACROs
                if (projectName == null) continue;

                //* Project Based MACROs
                if (parts[i] == PROJECT_MACROS[0])
                    results.Add(userDirectory);
                else if (parts[i] == PROJECT_MACROS[1])
                    results.Add(rootDirectory);
                else if (parts[i] == PROJECT_MACROS[2])
                    results.Add(projectDirectory);
                else if (parts[i] == PROJECT_MACROS[3])
                    results.Add(projectName);
                else
                    results.Add(parts[i]);
            }
            return new(command, [.. results]);
        }
    }
}