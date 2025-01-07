using Godot;
using System;
using System.Collections.Generic;

public partial class QuickToolRoot : VBoxContainer
{
    private VBoxContainer _container;

    private Dictionary<string, Button> _buttons = [];

    public override void _Ready()
    {
        _container = GetNode<VBoxContainer>("%QuickToolContainer");
    }

    public void SetQuickToolList()
    {
        foreach (KeyValuePair<string, Button> entry in _buttons)
        {
            if (entry.Value.IsQueuedForDeletion()) continue;

            entry.Value.QueueFree();
        }
        _buttons.Clear();

        string[] tags = TagCache.Instance.FavoritedSoftwareTags;
        if (tags.Length == 0)
        {
            _container.Hide();
            return;
        }

        foreach (string tag in tags)
        {
            Button buttonInstance = new();
            buttonInstance.Text = tag;
            _container.AddChild(buttonInstance);
            buttonInstance.Pressed += () => OSAPI.RunTool(tag, "");
            _buttons.Add(tag, buttonInstance);
        }
        _container.Show();
    }
}
