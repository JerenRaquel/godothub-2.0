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
        if (TagCache.Instance.HasProjectTag(text))
        {
            _colorPickerButton.Color = GetColor(false, text);
            tagExists = true;
        }
        else if (TagCache.Instance.HasSoftwareTag(text))
        {
            _colorPickerButton.Color = GetColor(true, text);
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
        Color color;
        if (TagCache.Instance.HasProjectTag(text))
            color = GetColor(false, text);
        else if (TagCache.Instance.HasSoftwareTag(text))
            color = GetColor(true, text);
        else    // Requires new Project Tag
        {
            color = _colorPickerButton.Color;
            TagCache.Instance.AddOrUpdateProjectTag(
                text,
                color.ToHtml()
            );
        }

        EmitSignal(SignalName.TagAdded, text, color);
        Hide();
    }

    private static Color GetColor(bool isSoftwareTag, string tag)
    {
        string colorCode = TagCache.Instance.GetColor(isSoftwareTag, tag);
        return new(colorCode);
    }
}
