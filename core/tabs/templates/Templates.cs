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
        _tagDisplay.TagRemoved += OnTagRemoved;

        _treeDisplay = GetNode<TreeDisplay>("%TreeDisplay");
        _treeDisplay.FillFolderStateToggled += () => TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate).FillFolders = _treeDisplay.FillFolders;

        _tagPrompt = GetNode<TagPrompt>("%TagPrompt");
        _tagPrompt.TagAdded += OnTagAdded;
    }

    public override void LoadData()
    {
        foreach (string templateName in TemplateCache.Instance.TemplateNames)
            _templateList.AddTemplate(templateName);
        OnNewFocus(_templateList.ActiveTemplate);
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
        // TODO: Uncomment once template editting is finished
        // _treeDisplay.ToggleEditting(!TemplateCache.DEFAULT_TEMPLATES.Contains(templateName));
    }

    private void OnTagAdded(string tagName, Color color)
    {
        TemplateStructure template = TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate);
        template.AddProjectTag(tagName);
        _tagDisplay.LoadTags(template);
    }

    private void OnTagRemoved()
    {
        foreach (ClickableTag tagInstance in _tagDisplay.SelectedTags)
        {
            if (tagInstance.IsSoftware)
                TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate).RemoveSoftwareTag(tagInstance.Text);
            else
                TemplateCache.Instance.GetTemplate(_templateList.ActiveTemplate).RemoveProjectTag(tagInstance.Text);
            tagInstance.QueueFree();
        }
        _tagDisplay.ClearRemovedTags();
    }
}
