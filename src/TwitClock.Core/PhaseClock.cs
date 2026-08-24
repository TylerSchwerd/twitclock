namespace TwitClock.Core;

public sealed class PhaseClock
{
    public static readonly TimeSpan ContentDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan AdBreakDuration = TimeSpan.FromSeconds(90);

    public PhaseClock(TimeSpan elapsed)
    {
        Phase = ClockPhase.Content;
        EndsAtElapsed = elapsed + ContentDuration;
    }

    public ClockPhase Phase { get; private set; }

    public TimeSpan EndsAtElapsed { get; private set; }

    public int GetRemainingSeconds(TimeSpan elapsed)
    {
        double seconds = (EndsAtElapsed - elapsed).TotalSeconds;
        return seconds <= 0 ? 0 : (int)Math.Ceiling(seconds);
    }

    public bool AdvanceIfExpired(TimeSpan elapsed)
    {
        if (elapsed < EndsAtElapsed)
        {
            return false;
        }

        SwitchPhase(elapsed);
        return true;
    }

    public void SwitchPhase(TimeSpan elapsed)
    {
        Phase = Phase == ClockPhase.Content
            ? ClockPhase.AdBreak
            : ClockPhase.Content;

        EndsAtElapsed = elapsed + GetDuration(Phase);
    }

    public void Adjust(TimeSpan adjustment, TimeSpan elapsed)
    {
        TimeSpan baseline = EndsAtElapsed < elapsed ? elapsed : EndsAtElapsed;
        TimeSpan adjustedEnd = baseline + adjustment;
        EndsAtElapsed = adjustedEnd < elapsed ? elapsed : adjustedEnd;
    }

    private static TimeSpan GetDuration(ClockPhase phase)
    {
        return phase == ClockPhase.Content ? ContentDuration : AdBreakDuration;
    }
}
