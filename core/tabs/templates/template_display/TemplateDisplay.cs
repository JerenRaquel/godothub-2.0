using Godot;
using System;

public partial class TemplateDisplay : MarginContainer
{
    private Tree _tree;

    public override void _Ready() => _tree = GetNode<Tree>("%Tree");

    public void Build(string templateName)
    {
        if (templateName == null) return;

        _tree.Clear();
        TemplateStructure.Folder rootFolder = TemplateCache.Instance.GetTemplateRootFolder(templateName);
        if (rootFolder.IsNull) return;

        TreeItem root = _tree.CreateItem();
        root.SetText(0, "[Unnamed Project]");
        BuildHelper(root, rootFolder);
    }

    private static void ReparentChild(TreeItem oldParent, TreeItem newParent, TreeItem child)
    {
        if (oldParent == newParent) return;

        oldParent.RemoveChild(child);
        newParent.AddChild(child);
    }

    private static TreeItem AddChild(TreeItem parent, string text, string colorCode = null)
    {
        TreeItem child = parent.CreateChild();
        child.SetText(0, text);
        if (colorCode != null) child.SetCustomColor(0, new(colorCode));

        return child;
    }

    private static void BuildHelper(TreeItem currentRoot, TemplateStructure.Folder currentFolder)
    {
        foreach (string fileName in currentFolder.FileNames)
        {
            string colorCode = null;
            if (fileName == "project.godot" || fileName == "icon.svg") colorCode = ColorTheme.FadedYellow;
            else if (fileName.StartsWith('.')) colorCode = ColorTheme.FadedGray;

            AddChild(currentRoot, fileName, colorCode);
        }

        foreach (string folderName in currentFolder.FolderNames)
        {
            TreeItem newRoot = AddChild(currentRoot, folderName);
            TemplateStructure.Folder subFolder = currentFolder.GetFolder(folderName);
            if (subFolder.FolderCount > 0)
            {
                BuildHelper(newRoot, subFolder);
            }
        }
    }
}
