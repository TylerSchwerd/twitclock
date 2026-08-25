using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TwitClock.Core;

namespace TwitClock.Ui.Tests;

internal static class Program
{
    private static int _failed;

    [STAThread]
    private static int Main()
    {
        Run("Maps every documented keyboard shortcut", MapsDocumentedKeyboardShortcuts);
        Run("Suppresses repeated phase switches and close commands", SuppressesRepeatedCommands);
        Run("Keyboard commands drive the live window", KeyboardCommandsDriveWindow);
        Run("Animates only when the displayed phase changes", AnimatesOnlyOnPhaseChanges);

        Console.WriteLine();
        Console.WriteLine(_failed == 0
            ? "All TwitClock Windows UI tests passed."
            : $"{_failed} TwitClock Windows UI test(s) failed.");

        return _failed == 0 ? 0 : 1;
    }

    private static void MapsDocumentedKeyboardShortcuts()
    {
        Equal(ClockCommand.AddMinute, KeyboardInput.Resolve(Key.OemPlus, false).Command);
        Equal(ClockCommand.AddMinute, KeyboardInput.Resolve(Key.Add, false).Command);
        Equal(ClockCommand.AddMinute, KeyboardInput.Resolve(Key.Up, false).Command);

        Equal(ClockCommand.SubtractMinute, KeyboardInput.Resolve(Key.OemMinus, false).Command);
        Equal(ClockCommand.SubtractMinute, KeyboardInput.Resolve(Key.Subtract, false).Command);
        Equal(ClockCommand.SubtractMinute, KeyboardInput.Resolve(Key.Down, false).Command);

        Equal(ClockCommand.SwitchPhase, KeyboardInput.Resolve(Key.Left, false).Command);
        Equal(ClockCommand.SwitchPhase, KeyboardInput.Resolve(Key.Right, false).Command);
        Equal(ClockCommand.Close, KeyboardInput.Resolve(Key.X, false).Command);

        Equal(false, KeyboardInput.Resolve(Key.F1, false).IsHandled);
    }

    private static void SuppressesRepeatedCommands()
    {
        KeyboardInputAction repeatedLeft = KeyboardInput.Resolve(Key.Left, true);
        Equal(true, repeatedLeft.IsHandled);
        Equal(ClockCommand.None, repeatedLeft.Command);

        KeyboardInputAction repeatedRight = KeyboardInput.Resolve(Key.Right, true);
        Equal(true, repeatedRight.IsHandled);
        Equal(ClockCommand.None, repeatedRight.Command);

        KeyboardInputAction repeatedClose = KeyboardInput.Resolve(Key.X, true);
        Equal(true, repeatedClose.IsHandled);
        Equal(ClockCommand.None, repeatedClose.Command);

        Equal(ClockCommand.AddMinute, KeyboardInput.Resolve(Key.Up, true).Command);
        Equal(ClockCommand.SubtractMinute, KeyboardInput.Resolve(Key.Down, true).Command);
    }

    private static void KeyboardCommandsDriveWindow()
    {
        MainWindow window = new();

        try
        {
            Equal(ClockPhase.Content, window.CurrentPhase);
            int initialSeconds = window.RemainingSeconds;
            Between(initialSeconds, 899, 900);

            Equal(true, window.ProcessKeyboardInput(Key.Up, false));
            int addedSeconds = window.RemainingSeconds;
            Between(addedSeconds - initialSeconds, 59, 60);

            Equal(true, window.ProcessKeyboardInput(Key.Down, false));
            int subtractedSeconds = window.RemainingSeconds;
            Between(addedSeconds - subtractedSeconds, 59, 60);

            Equal(true, window.ProcessKeyboardInput(Key.Left, false));
            Equal(ClockPhase.AdBreak, window.CurrentPhase);

            Equal(true, window.ProcessKeyboardInput(Key.Right, true));
            Equal(ClockPhase.AdBreak, window.CurrentPhase);

            Equal(true, window.ProcessKeyboardInput(Key.Right, false));
            Equal(ClockPhase.Content, window.CurrentPhase);

            Equal(false, window.ProcessKeyboardInput(Key.F1, false));
        }
        finally
        {
            window.Close();
        }
    }

    private static void AnimatesOnlyOnPhaseChanges()
    {
        Border target = new()
        {
            Background = new SolidColorBrush(Colors.Black)
        };
        PhaseBackgroundController controller = new(target);

        Equal(PhaseBackgroundUpdate.Initial, controller.Update(ClockPhase.Content));

        SolidColorBrush brush = RequireBrush(target);
        Equal(PhaseBackgroundController.ContentColor, brush.Color);
        Equal(false, brush.HasAnimatedProperties);

        Equal(PhaseBackgroundUpdate.Transition, controller.Update(ClockPhase.AdBreak));
        Equal(true, brush.HasAnimatedProperties);
        Same(brush, target.Background);

        Equal(PhaseBackgroundUpdate.None, controller.Update(ClockPhase.AdBreak));
        Equal(true, brush.HasAnimatedProperties);
        Same(brush, target.Background);

        Equal(TimeSpan.FromMilliseconds(500), PhaseBackgroundController.TransitionDuration);
    }

    private static SolidColorBrush RequireBrush(Border target)
    {
        return target.Background as SolidColorBrush
            ?? throw new InvalidOperationException("Expected a solid colour background brush.");
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine($"PASS  {name}");
        }
        catch (Exception exception)
        {
            _failed++;
            Console.Error.WriteLine($"FAIL  {name}");
            Console.Error.WriteLine($"      {exception.Message}");
        }
    }

    private static void Equal<T>(T expected, T actual)
        where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected {expected}, but received {actual}.");
        }
    }

    private static void Same(object expected, object? actual)
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException("Expected both references to point to the same object.");
        }
    }

    private static void Between(int actual, int minimum, int maximum)
    {
        if (actual < minimum || actual > maximum)
        {
            throw new InvalidOperationException(
                $"Expected a value from {minimum} through {maximum}, but received {actual}.");
        }
    }
}
