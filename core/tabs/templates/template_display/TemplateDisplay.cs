using Godot;
using System;

public partial class TemplateDisplay : MarginContainer
{
    private Tree _tree;

    public override void _Ready()
    {
        _tree = GetNode<Tree>("%Tree");

        CreateTree();
    }

    private TreeItem CreateTree()
    {
        TreeItem root = _tree.CreateItem();
        root.SetText(0, "[Unnamed Project]");

        TreeItem childA = AddChild(root, "icon.svg", ColorTheme.FadedGray);
        TreeItem childB = AddChild(root, "project.godot", ColorTheme.FadedGray);

        return root;
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
