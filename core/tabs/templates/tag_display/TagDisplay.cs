using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TagDisplay : VBoxContainer
{
    [Signal] public delegate void TagAddedEventHandler();
    [Signal] public delegate void TagRemovedEventHandler();
    [Signal] public delegate void TagContainerEnabledEventHandler();
    [Signal] public delegate void TagContainerDisabledEventHandler();

    [Export] private PackedScene TagScene;

    private Label _tagLabel;
    private Button _addButton;
    private Button _removeButton;
    private HBoxContainer _tagRootContainer;
    private HFlowContainer _tagContainer;

    private readonly List<ClickableTag> _selectedTags = [];

    public ClickableTag[] SelectedTags => [.. _selectedTags];

    public override void _Ready()
    {
        _tagLabel = GetNode<Label>("%TagLabel");

        _addButton = GetNode<Button>("%AddTagButton");
        _addButton.Pressed += () => EmitSignal(SignalName.TagAdded);

        _removeButton = GetNode<Button>("%RemoveTagButton");
        _removeButton.Pressed += () => EmitSignal(SignalName.TagRemoved);

        _tagRootContainer = GetNode<HBoxContainer>("%TagContainerRoot");

        _tagContainer = GetNode<HFlowContainer>("%TagContainer");
    }

    public void LoadTags(TemplateStructure template)
    {
        if (template == null || !template.HasTags)
        {
            DisplayTagTitle(false);
            return;
        }

        DisplayTagTitle(true);
        foreach (ClickableTag tag in _tagContainer.GetChildren().Cast<ClickableTag>())
        {
            if (tag.IsQueuedForDeletion()) continue;
            tag.QueueFree();
        }

        LoadTagsFromArray(template.ProjectTags, false);
        LoadTagsFromArray(template.SoftwareTags, true);
    }

    public void ClearRemovedTags()
    {
        _selectedTags.Clear();

        foreach (ClickableTag tag in _tagContainer.GetChildren())
        {
            if (tag.IsQueuedForDeletion()) continue;

            DisplayTagTitle(true);
            return;
        }
        DisplayTagTitle(false);
    }

    private void DisplayTagTitle(bool hasTags)
    {
        if (hasTags)
        {
            _tagLabel.Text = "Tags:";
            EmitSignal(SignalName.TagContainerEnabled);
            _removeButton.Show();
        }
        else
        {
            _tagLabel.Text = "Tags: No Tags Added";
            EmitSignal(SignalName.TagContainerDisabled);
            _removeButton.Hide();
        }
    }

    private void LoadTagsFromArray(string[] tags, bool isSoftware)
    {
        foreach (string tagName in tags)
        {
            ClickableTag tagInstance = TagScene.Instantiate<ClickableTag>();
            _tagContainer.AddChild(tagInstance);
            tagInstance.SetData(
                tagName,
                TagCache.Instance.GetColor(isSoftware, tagName),
                true
            );
            tagInstance.IsSoftware = isSoftware;
            tagInstance.Toggled += () => OnToggled(tagInstance);
        }
    }

    private void OnToggled(ClickableTag tagInstance)
    {
        if (tagInstance.Selected)
            _selectedTags.Add(tagInstance);
        else    //* Removed
            _selectedTags.Remove(tagInstance);
    }
}
