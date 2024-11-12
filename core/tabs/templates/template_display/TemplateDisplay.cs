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
        TemplateData.DataNode rootData = TemplateCache.Instance.GetRoot(templateName);
        if (rootData.IsNull) return;

        TreeItem root = _tree.CreateItem();
        root.SetText(0, "[Unamed Project]");

        foreach (string file in rootData.Files)
        {
            string colorCode = null;
            if (file == "project.godot" || file == "icon.svg") colorCode = ColorTheme.FadedYellow;
            else if (file.StartsWith('.')) colorCode = ColorTheme.FadedGray;

            AddChild(root, file, colorCode);
        }

    }

    private TreeItem AddChild(TreeItem parent, string text, string colorCode = null)
    {
        TreeItem child = parent.CreateChild();
        child.SetText(0, text);
        if (colorCode != null) child.SetCustomColor(0, new(colorCode));

        return child;
    }

    private static void ReparentChild(TreeItem oldParent, TreeItem newParent, TreeItem child)
    {
        if (oldParent == newParent) return;

        oldParent.RemoveChild(child);
        newParent.AddChild(child);
    }
}
