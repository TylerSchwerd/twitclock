using System.Windows.Input;

namespace TwitClock;

internal enum ClockCommand
{
    None,
    AddMinute,
    SubtractMinute,
    SwitchPhase,
    Close
}

internal readonly record struct KeyboardInputAction(bool IsHandled, ClockCommand Command);

internal static class KeyboardInput
{
    internal static KeyboardInputAction Resolve(Key key, bool isRepeat)
    {
        return key switch
        {
            Key.Add or Key.OemPlus or Key.Up => new(true, ClockCommand.AddMinute),
            Key.Subtract or Key.OemMinus or Key.Down => new(true, ClockCommand.SubtractMinute),
            Key.Left or Key.Right => new(true, isRepeat ? ClockCommand.None : ClockCommand.SwitchPhase),
            Key.X => new(true, isRepeat ? ClockCommand.None : ClockCommand.Close),
            _ => new(false, ClockCommand.None)
        };
    }
}
