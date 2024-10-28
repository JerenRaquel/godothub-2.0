using Godot;

[GlobalClass]
public partial class WindowBase : PanelContainer
{
    private Button _confirmButton;
    private Button _cancelButton;
    private VBoxContainer _notificationContainer;
    private TextureRect _notificationIcon;
    private Label _notificationLabel;

    [Export] private Texture2D errorIcon;
    [Export] private Texture2D passIcon;

    public override void _ExitTree()
    {
        base._ExitTree();
        _confirmButton.Pressed -= OnConfirmPressed;
        _cancelButton.Pressed -= OnCancelPressed;
    }

    public override void _Ready()
    {
        base._Ready();

        _confirmButton = GetNode<Button>("%ConfirmButton");
        _confirmButton.Pressed += OnConfirmPressed;
        _cancelButton = GetNode<Button>("%CancelButton");
        _cancelButton.Pressed += OnCancelPressed;

        _notificationContainer = GetNode<VBoxContainer>("%NotificationBox");
        _notificationIcon = GetNode<TextureRect>("%NotificationIcon");
        _notificationLabel = GetNode<Label>("%NotificationLabel");

        ClearWindowData();
        Hide();
    }

    protected void DisplayError(string message)
    {
        _notificationIcon.Texture = errorIcon;
        _notificationLabel.Text = message;
        _notificationContainer.Show();
    }

    protected void DisplayMessage(string message)
    {
        _notificationIcon.Texture = passIcon;
        _notificationLabel.Text = message;
        _notificationContainer.Show();
    }

    protected void ClearMessage()
    {
        _notificationContainer.Hide();
        _notificationIcon.Texture = errorIcon;
        _notificationLabel.Text = "Error";
    }

    protected virtual void ClearWindowData()
    {
        ClearMessage();
        _confirmButton.Disabled = true;
    }

    protected virtual void OnConfirmPressed() => GD.Print("Window Confirm Pressed");

    private void OnCancelPressed()
    {
        Hide();
        ClearWindowData();
    }
}
