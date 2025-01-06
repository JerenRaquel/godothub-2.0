using System.Collections.Generic;

public partial class TemplateStructure
{
    private List<string> _projectTags = [];
    private List<string> _softwareTags = [];
    private Folder _root;

    public string Name { get; set; }
    public string[] ProjectTags => [.. _projectTags];
    public string[] SoftwareTags => [.. _softwareTags];
    public long FolderCount => FolderCountHelper(_root);
    public long FileCount => FileCountHelper(_root);
    public Folder RootFolder => _root;

    public TemplateStructure(string name)
    {
        Name = name;
        _root = new("");
    }

    public void AddFile(string path, string fileTag)
    {
        string[] pathParts = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

        // Check if root folder
        if (pathParts.Length == 0)
        {
            _root.AddFile(fileTag);
            return;
        }

        // Sub Folder
        AddFolderHelper(ref _root, ref pathParts, 0, ref fileTag);
    }

    public void AddFolder(string path, string folderName)
    {
        string[] pathParts = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

        // Check if root folder
        if (pathParts.Length == 0)
        {
            _root.AddFolder(folderName);
            return;
        }

        // Sub Folder
        AddFolderHelper(ref _root, ref pathParts, 0, ref folderName);
    }

    private static string AddFileHelper(ref Folder currentFolder, ref string[] pathParts, int depth, ref string fileTag)
    {
        // If at the end of path
        if (depth == pathParts.Length - 1)
            return currentFolder.AddFile(fileTag);
        // Check if the child folder is in the current folder level, return if not
        if (!currentFolder.FolderExists(pathParts[depth + 1])) return null;

        // Check sub folders
        foreach (string subFolderName in currentFolder.FolderNames)
        {
            Folder subFolder = currentFolder.GetFolder(subFolderName);
            if (subFolder.IsNull) continue;

            return AddFileHelper(ref subFolder, ref pathParts, depth + 1, ref fileTag);
        }

        // Failed
        return null;
    }

    private static string AddFolderHelper(ref Folder currentFolder, ref string[] pathParts, int depth, ref string folderName)
    {
        // If at the end of path
        if (depth == pathParts.Length - 1)
            return currentFolder.AddFolder(folderName);
        // Check if the child folder is in the current folder level, return if not
        if (!currentFolder.FolderExists(pathParts[depth + 1])) return null;

        // Check sub folders
        foreach (string subFolderName in currentFolder.FolderNames)
        {
            Folder subFolder = currentFolder.GetFolder(subFolderName);
            if (subFolder.IsNull) continue;

            return AddFolderHelper(ref subFolder, ref pathParts, depth + 1, ref folderName);
        }

        // Failed
        return null;
    }

    private static int FolderCountHelper(Folder currentFolder)
    {
        if (currentFolder.IsNull) return 0;
        if (currentFolder.FolderCount == 0) return 0;

        int amount = currentFolder.FolderCount;
        foreach (string folderName in currentFolder.FolderNames)
            amount += FolderCountHelper(currentFolder.GetFolder(folderName));
        return amount;
    }

    private static int FileCountHelper(Folder currentFolder)
    {
        if (currentFolder.IsNull) return 0;
        if (currentFolder.FileCount == 0) return 0;

        int amount = currentFolder.FileCount;
        foreach (string folderName in currentFolder.FolderNames)
            amount += FolderCountHelper(currentFolder.GetFolder(folderName));
        return amount;
    }

}