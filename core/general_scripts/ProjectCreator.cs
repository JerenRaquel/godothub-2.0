
using System.Collections.Generic;
using System.IO;
using Godot;

public static partial class ProjectCreator
{
    public readonly struct ProjectCreationData(string name, bool isCSharp, string renderer, string version)
    {
        public readonly string Name = name;
        public readonly bool IsCSharp = isCSharp;
        public readonly string Renderer = renderer;
        public readonly string Version = version;
    }

    public static bool CreateProject(string path, string templateTag, ProjectCreationData data)
    {
        TemplateStructure template = TemplateCache.Instance.GetTemplate(templateTag);
        if (template.RootFolder.IsNull)
        {
            NotifcationManager.Instance.NotifyError("Unable to generate project from template.");
            return false;
        }

        OSAPI.CreateDirectoryIfNotExists(path);
        bool successState = GenerateFiles(path, path, template.RootFolder, data, template.FillFolders, templateTag);
        if (successState)
            NotifcationManager.Instance.NotifyValid("Project Created!");
        else
            NotifcationManager.Instance.NotifyError("Project Creation Failed!");
        return successState;
    }

    private static bool GenerateFiles(string projectPath, string currentPath,
        TemplateStructure.Folder currentFolder, ProjectCreationData data, bool fillEmptyFolders, string templateTag)
    {
        bool folderEmpty = true;
        foreach (string fileName in currentFolder.FileNames)
        {
            if (fileName == "project.godot")
            {
                if (!CreateProjectGodot(currentPath, data, templateTag))
                {
                    Abort(projectPath, "Couldn't generate a project.godot file");
                    return false;
                }
            }
            else if (fileName == ".gitignore")
            {
                if (!CreateGitIgnore(currentPath))
                {
                    Abort(projectPath, "Couldn't generate a .gitignore file");
                    return false;
                }
            }
            else if (fileName == ".gitattributes")
            {
                if (!CreateGitAttributes(currentPath))
                {
                    Abort(projectPath, "Couldn't generate a .gitattributes file");
                    return false;
                }
            }
            else    //* Copy File to Location
            {
                string filePath = TemplateCache.Instance.GetFilePath(fileName);
                if (filePath == null)
                {
                    Abort(projectPath, $"Missing file: {fileName}");
                    return false;
                }

                if (!CreateFileFromPath(currentPath, fileName, filePath))
                {
                    Abort(projectPath, $"Couldn't generate file: {fileName}");
                    return false;
                }
                folderEmpty = false;
            }
        }

        foreach (string folderName in currentFolder.FolderNames)
        {
            TemplateStructure.Folder subFolder = currentFolder.GetFolder(folderName);
            string subFolderPath = $"{currentPath}/{folderName}";

            OSAPI.CreateDirectoryIfNotExists(subFolderPath);
            bool successState = GenerateFiles(projectPath, subFolderPath, subFolder, data, fillEmptyFolders, templateTag);
            if (!successState) return false;

            folderEmpty = false;
        }

        if (folderEmpty && fillEmptyFolders)
        {
            if (!CreateFillerFile(currentPath))
            {
                Abort(projectPath, "Couldn't generate a .tmp file");
                return false;
            }
        }
        return true;
    }

    private static void Abort(string projectPath, string message)
    {
        if (Directory.Exists(projectPath))
            Directory.Delete(projectPath, true);
        NotifcationManager.Instance.NotifyError($"{message}. Aborting creation.");
    }

    private static bool CreateProjectGodot(string path, ProjectCreationData data, string templateTag)
    {
        ConfigFile file = new();
        file.SetValue("application", "config/name", data.Name);

        string[] projectTags = TemplateCache.Instance.GetTemplate(templateTag).ProjectTags;
        ProjectDataState.UpdateConfig(file, data.Version, data.IsCSharp, data.Renderer, projectTags);
        return file.Save($"{path}/project.godot") == Error.Ok;
    }

    private static bool CreateGitIgnore(string path)
    {
        try
        {
            StreamWriter file = new($"{path}/.gitignore");
            file.WriteLine("# Godot 4+ specific ignores");
            file.WriteLine(".godot/");
            file.WriteLine("/android/");
            file.Close();
            return true;
        }
        catch (System.Exception)
        { return false; }
    }

    private static bool CreateGitAttributes(string path)
    {
        try
        {
            StreamWriter file = new($"{path}/.gitattributes");
            file.WriteLine("# Normalize EOL for all files that Git considers text files.");
            file.WriteLine("* text=auto eol=lf");
            file.Close();
            return true;
        }
        catch (System.Exception)
        { return false; }
    }

    private static bool CreateFillerFile(string location) => File.Create($"{location}/.tmp") != null;

    private static bool CreateFileFromPath(string location, string name, string filePath)
    {
        try
        {
            File.Copy(filePath, $"{location}/{name}");
            return true;
        }
        catch (System.Exception)
        { return false; }
    }
}