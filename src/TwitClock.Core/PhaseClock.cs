namespace TwitClock.Core;

public sealed class PhaseClock
{
    public static readonly TimeSpan ContentDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan AdBreakDuration = TimeSpan.FromSeconds(90);

    public PhaseClock(DateTimeOffset nowUtc)
    {
        Phase = ClockPhase.Content;
        EndsAtUtc = nowUtc + ContentDuration;
    }

    public ClockPhase Phase { get; private set; }

    public DateTimeOffset EndsAtUtc { get; private set; }

    public int GetRemainingSeconds(DateTimeOffset nowUtc)
    {
        double seconds = (EndsAtUtc - nowUtc).TotalSeconds;
        return seconds <= 0 ? 0 : (int)Math.Ceiling(seconds);
    }

    public bool AdvanceIfExpired(DateTimeOffset nowUtc)
    {
        if (nowUtc < EndsAtUtc)
        {
            return false;
        }

        SwitchPhase(nowUtc);
        return true;
    }

    public void SwitchPhase(DateTimeOffset nowUtc)
    {
        Phase = Phase == ClockPhase.Content
            ? ClockPhase.AdBreak
            : ClockPhase.Content;

        EndsAtUtc = nowUtc + GetDuration(Phase);
    }

    public void Adjust(TimeSpan adjustment, DateTimeOffset nowUtc)
    {
        DateTimeOffset baseline = EndsAtUtc < nowUtc ? nowUtc : EndsAtUtc;
        DateTimeOffset adjustedEnd = baseline + adjustment;
        EndsAtUtc = adjustedEnd < nowUtc ? nowUtc : adjustedEnd;
    }

    private static TimeSpan GetDuration(ClockPhase phase)
    {
        return phase == ClockPhase.Content ? ContentDuration : AdBreakDuration;
    }
}
