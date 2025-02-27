using System.Collections.Generic;

public partial class TemplateStructure
{
    private HashSet<string> _projectTags = [];
    private HashSet<string> _softwareTags = [];
    private Folder _root;

    public string Name { get; set; }
    public bool FillFolders { get; set; } = true;

    public string[] ProjectTags => [.. _projectTags];
    public int ProjectTagCount => _projectTags.Count;
    public string[] SoftwareTags => [.. _softwareTags];
    public int SoftwareTagCount => _softwareTags.Count;
    public bool HasTags => _projectTags.Count > 0 || _softwareTags.Count > 0;
    public long FolderCount => FolderCountHelper(_root);
    public long FileCount => FileCountHelper(_root);
    public Folder RootFolder => _root;

    public TemplateStructure(string name)
    {
        Name = name;
        _root = new("");
    }

    public bool ContainsTag(string tagName, bool isSoftware)
    {
        if (!HasTags) return false;

        if (isSoftware)
            return _softwareTags.Contains(tagName);
        return _projectTags.Contains(tagName);
    }

    public void BulkAddProjectTags(string[] tags)
    {
        foreach (string tag in tags)
        {
            if (_projectTags.Contains(tag)) continue;
            _projectTags.Add(tag);
        }
    }

    public void BulkAddSoftwareTags(string[] tags)
    {
        foreach (string tag in tags)
        {
            if (_softwareTags.Contains(tag)) continue;
            _softwareTags.Add(tag);
        }
    }

    public void AddProjectTag(string tag)
    {
        if (_projectTags.Contains(tag)) return;
        _projectTags.Add(tag);
    }

    public void AddSoftwareTag(string tag)
    {
        if (_softwareTags.Contains(tag)) return;
        _softwareTags.Add(tag);
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

    public bool RemoveProjectTag(string tag)
    {
        if (!_projectTags.Contains(tag)) return false;
        _projectTags.Remove(tag);
        return true;
    }

    public bool RemoveSoftwareTag(string tag)
    {
        if (!_softwareTags.Contains(tag)) return false;
        _softwareTags.Remove(tag);
        return true;
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