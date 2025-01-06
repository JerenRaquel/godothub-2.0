
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

    public static void CreateProject(string path, string templateTag, ProjectCreationData data)
    {
        // TemplateData.DataNode projectRoot = TemplateCache.Instance.GetRoot(templateTag);
        // if (projectRoot.IsNull)
        // {
        //     NotifcationManager.Instance.NotifyError("Unable to generate project from template.");
        //     return;
        // }

        // GenerateFiles(path, path, projectRoot, data);
    }

    // private static void GenerateFiles(string projectPath, string currentPath,
    //     TemplateData.DataNode currentNode, ProjectCreationData data)
    // {
    //     string[] fileNames = currentNode.Files;
    //     foreach (string fileName in fileNames)
    //     {
    //         string filePath = TemplateCache.Instance.GetFilePath(fileName);
    //         if (filePath == null)
    //         {
    //             Abort(projectPath, $"Missing file: {fileName}");
    //             return;
    //         }

    //         if (fileName == "project.godot")
    //             CreateProjectGodot(currentPath, data);
    //         else if (fileName == "icon.svg")
    //         {

    //         }
    //         else    // Copy File to Location
    //         {

    //         }
    //     }

    //     foreach (TemplateData.DataNode folder in currentNode.FolderNodes)
    //         GenerateFiles(projectPath, currentPath + "/" + folder.Name, folder, data);
    // }

    private static void Abort(string projectPath, string message)
    {
        Directory.Delete(projectPath, true);
        NotifcationManager.Instance.NotifyError($"{message}. Aborting creation.");
    }

    private static void CreateProjectGodot(string path, ProjectCreationData data)
    {
        ConfigFile file = new();
        file.SetValue("application", "config/name", data.Name);

        List<string> featureData = [];
        featureData.Add(data.Version);
        if (data.IsCSharp) featureData.Add("C#");
        featureData.Add(data.Renderer);
        file.SetValue("application", "config/features", featureData.ToArray());

        file.Save(path);
    }
}