using Godot;
using System;

public partial class Templates : TabBase
{
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

        // TODO: Replace with checking if there's any tags.
        // TODO: -- Defaults rn to there's no tags.
        _noTagAddedLabel.Show();
        _tagContainerRoot.Hide();
    }

    public override void LoadData()
    {
        foreach (string templateName in TemplateCache.Instance.TemplateNames)
            _templateList.AddTemplate(templateName);
        _templateDisplay.Build(_templateList.ActiveTemplate);
    }

    private void OnNewFocus(string templateName)
    {
        _templateDisplay.Build(templateName);
    }
}
