using Godot;
using System;
using System.Linq;

public partial class Templates : TabBase
{
    [Export] private PackedScene TagScene;

    private TemplateList _templateList;
    private Label _noTagAddedLabel;
    private HBoxContainer _tagContainerRoot;
    private HFlowContainer _tagContainer;
    private TemplateDisplay _templateDisplay;

    public override void _Ready()
    {
        _templateList = GetNode<TemplateList>("%TemplateList");
        _templateList.GainFocus += OnNewFocus;

        _noTagAddedLabel = GetNode<Label>("%NoTagLabel");
        _tagContainerRoot = GetNode<HBoxContainer>("%TagContainerRoot");
        _tagContainer = GetNode<HFlowContainer>("%TagContainer");
        _templateDisplay = GetNode<TemplateDisplay>("%TemplateDisplay");
    }

    public override void LoadData()
    {
        foreach (string templateName in TemplateCache.Instance.TemplateNames)
            _templateList.AddTemplate(templateName);
        _templateDisplay.Build(_templateList.ActiveTemplate);
        LoadTags();
    }

    private void LoadTags()
    {
        TemplateStructure template = TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate);
        if (template != null && template.HasTags)
        {
            _noTagAddedLabel.Hide();
            _tagContainerRoot.Show();

            //* Load Tags
            foreach (Tag tag in _tagContainer.GetChildren().Cast<Tag>())
            {
                if (tag.IsQueuedForDeletion()) continue;
                tag.QueueFree();
            }

            foreach (string tagName in template.ProjectTags)
            {
                Tag tagInstance = TagScene.Instantiate<Tag>();
                _tagContainer.AddChild(tagInstance);
                tagInstance.SetData(
                    tagName,
                    TagCache.Instance.GetColor(false, tagName),
                    true
                );
            }

            foreach (string tagName in template.SoftwareTags)
            {
                Tag tagInstance = TagScene.Instantiate<Tag>();
                _tagContainer.AddChild(tagInstance);
                tagInstance.SetData(
                    tagName,
                    TagCache.Instance.GetColor(true, tagName),
                    true
                );
            }

        }
        else
        {
            _noTagAddedLabel.Show();
            _tagContainerRoot.Hide();
        }
    }

    private void OnNewFocus(string templateName)
    {
        _templateDisplay.Build(templateName);
        LoadTags();
    }
}
