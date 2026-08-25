using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TwitClock.Core;

namespace TwitClock;

public partial class MainWindow : Window
{
    private readonly Stopwatch _stopwatch;
    private readonly PhaseClock _clock;
    private readonly DispatcherTimer _timer;
    private readonly PhaseBackgroundController _backgroundController;

    public MainWindow()
    {
        InitializeComponent();

        _stopwatch = Stopwatch.StartNew();
        _clock = new PhaseClock(_stopwatch.Elapsed);
        _backgroundController = new PhaseBackgroundController(RootBorder);
        _timer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        _timer.Tick += Timer_Tick;
        _timer.Start();

        UpdateInterface();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _clock.AdvanceIfExpired(_stopwatch.Elapsed);
        UpdateInterface();
    }

    private void AddMinute_Click(object sender, RoutedEventArgs e)
    {
        AdjustByOneMinute(1);
    }

    private void SubtractMinute_Click(object sender, RoutedEventArgs e)
    {
        AdjustByOneMinute(-1);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        Keyboard.Focus(this);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = ProcessKeyboardInput(e.Key, e.IsRepeat);
    }

    internal bool ProcessKeyboardInput(Key key, bool isRepeat)
    {
        KeyboardInputAction input = KeyboardInput.Resolve(key, isRepeat);
        if (!input.IsHandled)
        {
            return false;
        }

        switch (input.Command)
        {
            case ClockCommand.AddMinute:
                AdjustByOneMinute(1);
                break;

            case ClockCommand.SubtractMinute:
                AdjustByOneMinute(-1);
                break;

            case ClockCommand.SwitchPhase:
                SwitchPhase();
                break;

            case ClockCommand.Close:
                Close();
                break;
        }

        return true;
    }

    private void SwitchPhase_Click(object sender, RoutedEventArgs e)
    {
        SwitchPhase();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || FindAncestor<Button>(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        DragMove();
    }

    private void AdjustByOneMinute(int direction)
    {
        _clock.Adjust(TimeSpan.FromMinutes(direction), _stopwatch.Elapsed);
        UpdateInterface();
    }

    private void SwitchPhase()
    {
        _clock.SwitchPhase(_stopwatch.Elapsed);
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        TimeSpan elapsed = _stopwatch.Elapsed;
        int secondsRemaining = _clock.GetRemainingSeconds(elapsed);
        int minutes = secondsRemaining / 60;
        int seconds = secondsRemaining % 60;

        TimeText.Text = $"{minutes:00}:{seconds:00}";
        PhaseText.Text = _clock.Phase == ClockPhase.Content ? "CONTENT" : "AD BREAK";
        _backgroundController.Update(_clock.Phase);
    }

    internal ClockPhase CurrentPhase => _clock.Phase;

    internal int RemainingSeconds => _clock.GetRemainingSeconds(_stopwatch.Elapsed);

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
        base.OnClosed(e);
    }

    private static T? FindAncestor<T>(DependencyObject? current)
        where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
