using DataContainer.DatabaseSys.Databases.TagDatabase;
using Godot;
using System;
using System.Collections.Generic;

public partial class QuickToolRoot : VBoxContainer
{
    private VBoxContainer _container;

    private Dictionary<TagKey, Button> _buttons = [];

    public override void _Ready()
    {
        _container = GetNode<VBoxContainer>("%QuickToolContainer");
    }

    public void SetQuickToolList()
    {
        foreach (KeyValuePair<TagKey, Button> entry in _buttons)
        {
            if (entry.Value.IsQueuedForDeletion()) continue;

            entry.Value.QueueFree();
        }
        _buttons.Clear();

        TagKey[] tagKeys = TagDatabase.Instance.GetFavoritedSoftwareTags();
        if (tagKeys.Length == 0)
        {
            _container.Hide();
            return;
        }

        foreach (TagKey tagKey in tagKeys)
        {
            Button buttonInstance = new();
            buttonInstance.Text = tagKey.TagName;
            _container.AddChild(buttonInstance);
            buttonInstance.Pressed += () => OSAPI.RunTool(tagKey, "");
            _buttons.Add(tagKey, buttonInstance);
        }
        _container.Show();
    }
}
