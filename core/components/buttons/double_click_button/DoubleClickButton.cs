using Godot;
using System;

public partial class DoubleClickButton : Button
{
    [Signal] public delegate void LaunchRequestedEventHandler();
    [Signal] public delegate void StateToggledEventHandler(bool state);

    private Timer _timer;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");

        Toggled += OnMainToggled;
    }

    public void ToggleOff() => SetPressedNoSignal(false);

    private void OnMainToggled(bool toggled_on)
    {
        if (_timer.TimeLeft > 0.0)
        {
            _timer.Stop();
            EmitSignal(SignalName.LaunchRequested);
            //* Must come after launch signal -- Hence the two lines of toggle emitting
            EmitSignal(SignalName.StateToggled, toggled_on);
            return;
        }
        EmitSignal(SignalName.StateToggled, toggled_on);
        _timer.Start();
    }
}
