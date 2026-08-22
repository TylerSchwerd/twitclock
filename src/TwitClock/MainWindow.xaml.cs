using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TwitClock.Core;

namespace TwitClock;

public partial class MainWindow : Window
{
    private static readonly Color ContentGreen = Color.FromRgb(33, 196, 94);
    private static readonly Color AdBreakRed = Color.FromRgb(240, 69, 69);

    private readonly PhaseClock _clock;
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        _clock = new PhaseClock(DateTimeOffset.UtcNow);
        _timer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        _timer.Tick += Timer_Tick;
        _timer.Start();

        UpdateInterface(animateBackground: false);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        bool phaseChanged = _clock.AdvanceIfExpired(nowUtc);
        UpdateInterface(animateBackground: phaseChanged);
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
        switch (e.Key)
        {
            case Key.Add:
            case Key.OemPlus:
            case Key.Up:
                AdjustByOneMinute(1);
                e.Handled = true;
                break;

            case Key.Subtract:
            case Key.OemMinus:
            case Key.Down:
                AdjustByOneMinute(-1);
                e.Handled = true;
                break;

            case Key.Left:
            case Key.Right:
                if (!e.IsRepeat)
                {
                    SwitchPhase();
                }

                e.Handled = true;
                break;

            case Key.X:
                if (!e.IsRepeat)
                {
                    Close();
                }

                e.Handled = true;
                break;
        }
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
        _clock.Adjust(TimeSpan.FromMinutes(direction), DateTimeOffset.UtcNow);
        UpdateInterface(animateBackground: false);
    }

    private void SwitchPhase()
    {
        _clock.SwitchPhase(DateTimeOffset.UtcNow);
        UpdateInterface(animateBackground: true);
    }

    private void UpdateInterface(bool animateBackground)
    {
        int secondsRemaining = _clock.GetRemainingSeconds(DateTimeOffset.UtcNow);
        int minutes = secondsRemaining / 60;
        int seconds = secondsRemaining % 60;

        TimeText.Text = $"{minutes:00}:{seconds:00}";
        PhaseText.Text = _clock.Phase == ClockPhase.Content ? "CONTENT" : "AD BREAK";

        Color phaseColor = _clock.Phase == ClockPhase.Content ? ContentGreen : AdBreakRed;
        SetBackground(phaseColor, animateBackground);
    }

    private void SetBackground(Color color, bool animate)
    {
        if (RootBorder.Background is not SolidColorBrush currentBrush)
        {
            RootBorder.Background = new SolidColorBrush(color);
            return;
        }

        SolidColorBrush brush = currentBrush;
        if (brush.IsFrozen)
        {
            brush = currentBrush.CloneCurrentValue();
            RootBorder.Background = brush;
        }

        if (!animate)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            brush.Color = color;
            return;
        }

        ColorAnimation animation = new()
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

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
