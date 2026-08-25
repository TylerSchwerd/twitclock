using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TwitClock.Core;

namespace TwitClock;

internal enum PhaseBackgroundUpdate
{
    None,
    Initial,
    Transition
}

internal sealed class PhaseBackgroundController
{
    internal static readonly Color ContentColor = Color.FromRgb(33, 196, 94);
    internal static readonly Color AdBreakColor = Color.FromRgb(240, 69, 69);
    internal static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(500);

    private readonly Border _target;
    private ClockPhase? _displayedPhase;

    internal PhaseBackgroundController(Border target)
    {
        _target = target;
    }

    internal PhaseBackgroundUpdate Update(ClockPhase phase)
    {
        if (_displayedPhase == phase)
        {
            return PhaseBackgroundUpdate.None;
        }

        bool animate = _displayedPhase.HasValue;
        SetBackground(phase == ClockPhase.Content ? ContentColor : AdBreakColor, animate);
        _displayedPhase = phase;

        return animate ? PhaseBackgroundUpdate.Transition : PhaseBackgroundUpdate.Initial;
    }

    private void SetBackground(Color color, bool animate)
    {
        if (_target.Background is not SolidColorBrush currentBrush)
        {
            _target.Background = new SolidColorBrush(color);
            return;
        }

        SolidColorBrush brush = currentBrush;
        if (brush.IsFrozen)
        {
            brush = currentBrush.CloneCurrentValue();
            _target.Background = brush;
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
            Duration = TransitionDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }
}
