using Godot;
using System;

public partial class NotifcationManager : VBoxContainer
{
    #region Singleton Instance
    private static NotifcationManager _instance;
    public static NotifcationManager Instance => _instance;
    #endregion

    [Export] private PackedScene _notification;

    public override void _Ready()
    {
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;
    }

    public void NotifyValid(string message, double delay = 3f) => Notify(message, ColorTheme.Forward, delay);
    public void NotifyError(string message, double delay = 3f) => Notify(message, ColorTheme.Mobile, delay);
    public void NotifyWarning(string message, double delay = 3f) => Notify(message, ColorTheme.Unknown, delay);

    private void Notify(string message, string colorCode, double delay)
    {
        if (message == null || message.Length == 0) return;

        Notification notif = _notification.Instantiate<Notification>();
        AddChild(notif);
        MoveChild(notif, 0);
        notif.Start(message, colorCode, delay);
    }
}
