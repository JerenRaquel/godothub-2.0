using Godot;
using System;

public partial class Templates : TabBase
{
    private TemplateList _templateList;
    private TemplateDisplay _templateDisplay;

    public override void _Ready()
    {
        _templateList = GetNode<TemplateList>("%TemplateList");
        _templateList.GainFocus += OnNewFocus;
        _templateDisplay = GetNode<TemplateDisplay>("%TemplateDisplay");
    }

    public override void LoadData()
    {
        foreach (string templateName in TemplateCache.Instance.TemplateNames)
            _templateList.AddTemplate(templateName);
        GD.Print(_templateList.ActiveTemplate);
    }

    private void OnNewFocus(string templateName)
    {
        GD.Print(templateName);
    }
}
