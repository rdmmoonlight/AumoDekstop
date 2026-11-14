using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace AumoDekstop.Helpers;

public partial class LoginThrottleController : ObservableObject, IDisposable
{
    private readonly int _cooldownSeconds;
    private readonly DispatcherQueueTimer _timer;

    [ObservableProperty]
    private bool _isThrottled;

    [ObservableProperty]
    private int _remainingTime;

    public LoginThrottleController(int cooldownSeconds = 60, DispatcherQueue? dispatcherQueue = null)
    {
        _cooldownSeconds = cooldownSeconds;

        var queue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException("LoginThrottleController harus diinisialisasi di UI thread.");

        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnTimerTick;
    }

    public void StartThrottle(int? customSeconds = null)
    {
        _timer.Stop();
        IsThrottled = true;
        RemainingTime = customSeconds ?? _cooldownSeconds;
        _timer.Start();
    }

    public void ResetThrottle()
    {
        _timer.Stop();
        IsThrottled = false;
        RemainingTime = 0;
    }

    private void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        RemainingTime -= 1;
        if (RemainingTime <= 0)
        {
            ResetThrottle();
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
    }
}