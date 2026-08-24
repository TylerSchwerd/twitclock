using TwitClock.Core;

namespace TwitClock.Core.Tests;

internal static class Program
{
    private static int _failed;

    private static int Main()
    {
        Run("Starts with a fifteen-minute content phase", StartsWithContentPhase);
        Run("Counts down using monotonic elapsed time", CountsDown);
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
        TimeSpan elapsed = TimeSpan.FromHours(12);
        PhaseClock clock = new(elapsed);

        Equal(ClockPhase.Content, clock.Phase);
        Equal(900, clock.GetRemainingSeconds(elapsed));
    }

    private static void CountsDown()
    {
        TimeSpan elapsed = TimeSpan.FromDays(20);
        PhaseClock clock = new(elapsed);

        Equal(899, clock.GetRemainingSeconds(elapsed.Add(TimeSpan.FromSeconds(1))));
        Equal(1, clock.GetRemainingSeconds(elapsed.Add(TimeSpan.FromMilliseconds(899_100))));
        Equal(0, clock.GetRemainingSeconds(elapsed.Add(TimeSpan.FromMinutes(15))));
    }

    private static void SwitchesToAdBreak()
    {
        TimeSpan elapsed = TimeSpan.FromHours(12);
        PhaseClock clock = new(elapsed);

        clock.SwitchPhase(elapsed);

        Equal(ClockPhase.AdBreak, clock.Phase);
        Equal(90, clock.GetRemainingSeconds(elapsed));
    }

    private static void AdvancesWhenExpired()
    {
        TimeSpan elapsed = TimeSpan.FromHours(12);
        PhaseClock clock = new(elapsed);

        Equal(false, clock.AdvanceIfExpired(elapsed.Add(TimeSpan.FromSeconds(899))));
        Equal(true, clock.AdvanceIfExpired(elapsed.Add(TimeSpan.FromSeconds(900))));
        Equal(ClockPhase.AdBreak, clock.Phase);
        Equal(90, clock.GetRemainingSeconds(elapsed.Add(TimeSpan.FromSeconds(900))));
    }

    private static void AdjustsTime()
    {
        TimeSpan elapsed = TimeSpan.FromHours(12);
        PhaseClock clock = new(elapsed);

        clock.Adjust(TimeSpan.FromMinutes(1), elapsed);
        Equal(960, clock.GetRemainingSeconds(elapsed));

        clock.Adjust(TimeSpan.FromMinutes(-1), elapsed);
        Equal(900, clock.GetRemainingSeconds(elapsed));
    }

    private static void ClampsAtZero()
    {
        TimeSpan elapsed = TimeSpan.FromHours(12);
        PhaseClock clock = new(elapsed);

        clock.Adjust(TimeSpan.FromHours(-1), elapsed);

        Equal(0, clock.GetRemainingSeconds(elapsed));
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
