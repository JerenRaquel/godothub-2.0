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
    [Export] private Texture2D warningIcon;
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
        VisibilityChanged += () => { if (Visible) OnOpened(); };
    }

    protected void DisplayError(string message)
    {
        _notificationIcon.Texture = errorIcon;
        _notificationLabel.Text = message;
        _notificationLabel.AddThemeColorOverride("font_color", new Color(ColorTheme.Dev));
        _notificationContainer.Show();
        _confirmButton.Disabled = true;
    }

    protected void DisplayWarning(string message)
    {

        _notificationIcon.Texture = warningIcon;
        _notificationLabel.Text = message;
        _notificationLabel.AddThemeColorOverride("font_color", new Color(ColorTheme.Unknown));
        _notificationContainer.Show();
        _confirmButton.Disabled = false;
    }

    protected void DisplayMessage(string message)
    {
        _notificationIcon.Texture = passIcon;
        _notificationLabel.Text = message;
        _notificationLabel.AddThemeColorOverride("font_color", new Color(ColorTheme.Forward));
        _notificationContainer.Show();
        _confirmButton.Disabled = false;
    }

    protected void ForceConfirm()
    {
        ClearMessage();
        _confirmButton.Disabled = false;
    }

    protected void ClearMessage()
    {
        _notificationContainer.Hide();
        _notificationIcon.Texture = errorIcon;
        _notificationLabel.Text = "Error";
        _notificationContainer.RemoveThemeColorOverride("font_color");
    }

    protected virtual void ClearWindowData()
    {
        ClearMessage();
        _confirmButton.Disabled = true;
    }

    protected virtual bool Validate() => false;
    protected void OnPathTextUpdated(string _path) => Validate();
    protected void OnOptionUpdated(long _index) => Validate();
    protected void OnToggleUpdated(bool _state) => Validate();

    protected virtual void OnOpened() { }

    protected virtual void OnConfirmPressed() => GD.Print("Window Confirm Pressed");

    private void OnCancelPressed()
    {
        Hide();
        ClearWindowData();
    }
}
