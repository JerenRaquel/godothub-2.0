using System.IO;

public partial class TemplateStructure
{
    public void LoadFileData(ref string fileName, ref string fileTag, ref string path)
    {
        if (path == "")
        {
            string[] parts = [];
            LoadFileDataHelper(ref parts, 0, _root, ref fileName, ref fileTag);
        }
        else
        {
            string[] parts = path.Split("/");
            LoadFileDataHelper(ref parts, 0, _root, ref fileName, ref fileTag);
        }
    }

    public void OverwriteWith(TemplateStructure other)
    {
        _root = other._root;
        _projectTags = other._projectTags;
        _softwareTags = other._softwareTags;
        Name = other.Name;
    }

    private static void LoadFileDataHelper(ref string[] parts, int depth, Folder currentFolder,
        ref string fileName, ref string fileTag)
    {
        if (parts.Length == 0 || depth + 1 >= parts.Length)
        {
            currentFolder.LoadData(fileName, fileTag);
            return;
        }

        string nextFolderName = parts[depth + 1];
        if (!currentFolder.FolderExists(nextFolderName))
            currentFolder.AddFolder(nextFolderName);

        LoadFileDataHelper(
            ref parts,
            depth + 1,
            currentFolder.GetFolder(nextFolderName),
            ref fileName,
            ref fileTag
        );

    }
}