namespace Envelope.Timers;

public class SequentialSyncTimer : BaseSequentialSyncTimer
{
	private readonly Func<object?, bool> _timerCallback;
	private readonly Func<object?, Exception, bool>? _exceptionCallback;

	public SequentialSyncTimer(
		object? state,
		TimeSpan timerInterval,
		Func<object?, bool> timerCallback,
		Func<object?, Exception, bool>? exceptionCallback = null)
		: base(state, timerInterval)
	{
		_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
		_exceptionCallback = exceptionCallback;
	}

	public SequentialSyncTimer(
		object? state,
		TimeSpan startDelay,
		TimeSpan timerInterval,
		Func<object?, bool> timerCallback,
		Func<object?, Exception, bool>? exceptionCallback = null)
		: base(state, startDelay, timerInterval)
	{
		_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
		_exceptionCallback = exceptionCallback;
	}

	protected override bool OnTimer(object? state)
		=> _timerCallback(state);

	protected override bool OnError(object? state, Exception ex)
		=> _exceptionCallback?.Invoke(state, ex) ?? true;

	public static SequentialSyncTimer Start(
		object? state,
		TimeSpan timerInterval,
		Func<object?, bool> timerCallback,
		Func<object?, Exception, bool>? exceptionCallback = null)
	{
		var timer = new SequentialSyncTimer(state, timerInterval, timerCallback, exceptionCallback);
		timer.Start();
		return timer;
	}

	public static SequentialSyncTimer Start(
		object? state,
		TimeSpan startDelay,
		TimeSpan timerInterval,
		Func<object?, bool> timerCallback,
		Func<object?, Exception, bool>? exceptionCallback = null)
	{
		var timer = new SequentialSyncTimer(state, startDelay, timerInterval, timerCallback, exceptionCallback);
		timer.Start();
		return timer;
	}
}
