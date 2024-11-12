using Godot;
using System;

public partial class Templates : TabBase
{
    private TemplateList _templateList;
    private TemplateDisplay _templateDisplay;
    private TemplateSettings _templateSettings;

    public override void _Ready()
    {
        _templateList = GetNode<TemplateList>("%TemplateList");
        _templateList.GainFocus += OnNewFocus;
        _templateDisplay = GetNode<TemplateDisplay>("%TemplateDisplay");
        _templateSettings = GetNode<TemplateSettings>("%TemplateSettings");
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
