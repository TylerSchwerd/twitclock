using TwitClock.Core;

namespace TwitClock.Core.Tests;

internal static class Program
{
    private static int _failed;

    private static int Main()
    {
        Run("Starts with a fifteen-minute content phase", StartsWithContentPhase);
        Run("Counts down using elapsed time", CountsDown);
        Run("Switches to a ninety-second ad break", SwitchesToAdBreak);
        Run("Automatically advances when time expires", AdvancesWhenExpired);
        Run("Adjusts the remaining time by one minute", AdjustsTime);
        Run("Never adjusts below zero", ClampsAtZero);

        Console.WriteLine();
        Console.WriteLine(_failed == 0
            ? "All TwitClock core tests passed."
            : $"{_failed} TwitClock core test(s) failed.");

        return _failed == 0 ? 0 : 1;
    }

    private static void StartsWithContentPhase()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        Equal(ClockPhase.Content, clock.Phase);
        Equal(900, clock.GetRemainingSeconds(now));
    }

    private static void CountsDown()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        Equal(899, clock.GetRemainingSeconds(now.AddSeconds(1)));
        Equal(1, clock.GetRemainingSeconds(now.AddMilliseconds(899_100)));
        Equal(0, clock.GetRemainingSeconds(now.AddMinutes(15)));
    }

    private static void SwitchesToAdBreak()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        clock.SwitchPhase(now);

        Equal(ClockPhase.AdBreak, clock.Phase);
        Equal(90, clock.GetRemainingSeconds(now));
    }

    private static void AdvancesWhenExpired()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        Equal(false, clock.AdvanceIfExpired(now.AddSeconds(899)));
        Equal(true, clock.AdvanceIfExpired(now.AddSeconds(900)));
        Equal(ClockPhase.AdBreak, clock.Phase);
        Equal(90, clock.GetRemainingSeconds(now.AddSeconds(900)));
    }

    private static void AdjustsTime()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        clock.Adjust(TimeSpan.FromMinutes(1), now);
        Equal(960, clock.GetRemainingSeconds(now));

        clock.Adjust(TimeSpan.FromMinutes(-1), now);
        Equal(900, clock.GetRemainingSeconds(now));
    }

    private static void ClampsAtZero()
    {
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        PhaseClock clock = new(now);

        clock.Adjust(TimeSpan.FromHours(-1), now);

        Equal(0, clock.GetRemainingSeconds(now));
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
}
