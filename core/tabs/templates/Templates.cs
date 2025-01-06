using Godot;
using System.Linq;

public partial class Templates : TabBase
{
    private TemplateList _templateList;
    private VSplitContainer _splitContainer;
    private TagDisplay _tagDisplay;
    private TreeDisplay _treeDisplay;
    private TagPrompt _tagPrompt;

    public override void _Ready()
    {
        _templateList = GetNode<TemplateList>("%TemplateList");
        _templateList.GainFocus += OnNewFocus;

        _splitContainer = GetNode<VSplitContainer>("%VSplitContainer");

        _tagDisplay = GetNode<TagDisplay>("%TagDisplay");
        _tagDisplay.TagContainerEnabled += () => _splitContainer.Collapsed = false;
        _tagDisplay.TagContainerDisabled += () => _splitContainer.Collapsed = true;
        _tagDisplay.TagAdded += () => _tagPrompt.Show();

        _treeDisplay = GetNode<TreeDisplay>("%TreeDisplay");
        _treeDisplay.FillFolderStateToggled += () => TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate).FillFolders = _treeDisplay.FillFolders;

        _tagPrompt = GetNode<TagPrompt>("%TagPrompt");
        _tagPrompt.TagAdded += OnTagAdded;
    }

    public override void LoadData()
    {
        foreach (string templateName in TemplateCache.Instance.TemplateNames)
            _templateList.AddTemplate(templateName);
        _treeDisplay.Build(_templateList.ActiveTemplate);
        LoadTags();
    }

    private void LoadTags()
    {
        TemplateStructure template = TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate);
        _treeDisplay.FillFolders = template.FillFolders;
        _tagDisplay.LoadTags(template);
    }

    private void OnNewFocus(string templateName)
    {
        _treeDisplay.Build(templateName);
        LoadTags();
    }

    private void OnTagAdded(string tagName, Color color)
    {
        TemplateStructure template = TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate);
        template.AddProjectTag(tagName);
        _tagDisplay.LoadTags(template);
    }
}
