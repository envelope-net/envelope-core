﻿namespace Envelope.Timers;

public class SequentialAsyncTimer : BaseSequentialAsyncTimer
{
	private readonly Func<object?, Task<bool>> _timerCallback;
	private readonly Func<object?, Exception, Task<bool>>? _exceptionCallback;

	public SequentialAsyncTimer(
		object? state,
		Func<object?>? onTimerTickStateFunc,
		TimeSpan timerInterval,
		Func<object?, Task<bool>> timerCallback,
		Func<object?, Exception, Task<bool>>? exceptionCallback = null)
		: base(state, onTimerTickStateFunc, timerInterval)
	{
		_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
		_exceptionCallback = exceptionCallback;
	}

	public SequentialAsyncTimer(
		object? state,
		Func<object?>? onTimerTickStateFunc,
		TimeSpan startDelay,
		TimeSpan timerInterval,
		Func<object?, Task<bool>> timerCallback,
		Func<object?, Exception, Task<bool>>? exceptionCallback = null)
		: base(state, onTimerTickStateFunc, startDelay, timerInterval)
	{
		_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
		_exceptionCallback = exceptionCallback;
	}

	protected override Task<bool> OnTimerAsync(object? state)
		=> _timerCallback(state);

	protected override Task<bool> OnErrorAsync(object? state, Exception ex)
		=> _exceptionCallback != null
			? _exceptionCallback.Invoke(state, ex)
			: Task.FromResult(true);

	public static async Task<SequentialAsyncTimer> StartAsync(
		object? state,
		Func<object?>? onTimerTickStateFunc,
		TimeSpan timerInterval,
		Func<object?, Task<bool>> timerCallback,
		Func<object?, Exception, Task<bool>>? exceptionCallback = null)
	{
		var timer = new SequentialAsyncTimer(state, onTimerTickStateFunc, timerInterval, timerCallback, exceptionCallback);
		await timer.StartAsync();
		return timer;
	}

	public static async Task<SequentialAsyncTimer> StartAsync(
		object? state,
		Func<object?>? onTimerTickStateFunc,
		TimeSpan startDelay,
		TimeSpan timerInterval,
		Func<object?, Task<bool>> timerCallback,
		Func<object?, Exception, Task<bool>>? exceptionCallback = null)
	{
		var timer = new SequentialAsyncTimer(state, onTimerTickStateFunc, startDelay, timerInterval, timerCallback, exceptionCallback);
		await timer.StartAsync();
		return timer;
	}
}
