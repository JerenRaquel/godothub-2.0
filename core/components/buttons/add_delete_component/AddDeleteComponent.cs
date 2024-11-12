using Godot;

public partial class AddDeleteComponent : HBoxContainer
{
    [Signal] public delegate void AddPressedEventHandler();
    [Signal] public delegate void DeletePressedEventHandler();

    private Button _addButton;
    private Button _deleteButton;

    public bool Disabled
    {
        get => _deleteButton.Disabled && _addButton.Disabled;
        set
        {
            _addButton.Disabled = value;
            _deleteButton.Disabled = value;
        }
    }
    public bool DeleteDisabled
    {
        get => _deleteButton.ButtonPressed;
        set => _deleteButton.Disabled = value;
    }
    public bool AddDisabled
    {
        get => _addButton.ButtonPressed;
        set => _addButton.Disabled = value;
    }


    public override void _Ready()
    {
        _addButton = GetNode<Button>("%AddButton");
        _addButton.Pressed += () => EmitSignal(SignalName.AddPressed);
        _deleteButton = GetNode<Button>("%DeleteButton");
        _addButton.Pressed += () => EmitSignal(SignalName.DeletePressed);
    }
}
