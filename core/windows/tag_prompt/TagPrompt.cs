using DataContainer.DatabaseSys.Databases.TagDatabase;
using Godot;
using System;

public partial class TagPrompt : WindowBase
{
    [Signal] public delegate void TagAddedEventHandler(string name, Color color);

    private ColorPickerButton _colorPickerButton;
    private LineEdit _lineEdit;

    public override void _Ready()
    {
        _colorPickerButton = GetNode<ColorPickerButton>("%ColorPickerButton");

        _lineEdit = GetNode<LineEdit>("%LineEdit");
        _lineEdit.TextChanged += _ => Validate();

        base._Ready();
    }

    protected override void ClearWindowData()
    {
        base.ClearWindowData();
        _lineEdit.Clear();
    }

    protected override bool Validate()
    {
        string text = _lineEdit.Text;
        if (text == "" || text == null)
        {
            DisplayError("Text can't be empty.");
            return false;
        }

        bool tagExists = false;
        TagKey projectTag = new(text, false);
        TagKey softwareTag = new(text, false);
        if (TagDatabase.Instance.HasKey(projectTag))
        {
            _colorPickerButton.Color = GetColor(projectTag);
            tagExists = true;
        }
        else if (TagDatabase.Instance.HasKey(softwareTag))
        {
            _colorPickerButton.Color = GetColor(softwareTag);
            tagExists = true;
        }

        if (tagExists)
            DisplayMessage("Will add as existing tag.");
        else
            DisplayWarning("Tag Doesn't Exists... This will add it as a new Project Tag.");

        return true;
    }

    protected override void OnConfirmPressed()
    {
        if (!Validate()) return;

        string text = _lineEdit.Text;
        TagKey projectTag = new(text, false);
        TagKey softwareTag = new(text, false);
        Color color;
        if (TagDatabase.Instance.HasKey(projectTag))
            color = GetColor(projectTag);
        else if (TagDatabase.Instance.HasKey(softwareTag))
            color = GetColor(softwareTag);
        else    // Requires new Project Tag
        {
            color = _colorPickerButton.Color;
            TagDatabase.Instance.AddOrUpdate(in projectTag, new(false, color.ToHtml()));
        }

        EmitSignal(SignalName.TagAdded, text, color);
        Hide();
    }

    private static Color GetColor(in TagKey key)
    {
        string htmlColor = TagDatabase.Instance.GetHTMLColor(key);
        return new(htmlColor);
    }
}
