using Envelope.Threading;

namespace Envelope.Timers;

public abstract class BaseSequentialAsyncTimer : IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	protected readonly Timer _timer;
	private bool disposed;

	public TimeSpan StartDelay { get; set; }
	public TimeSpan TimerInterval { get; set; }
	public bool IsInProcess { get; private set; }
	public bool IsActive { get; private set; }

	public BaseSequentialAsyncTimer(object? state, TimeSpan timerInterval)
		: this(state, TimeSpan.Zero, timerInterval)
	{
	}

	public BaseSequentialAsyncTimer(object? state, TimeSpan startDelay, TimeSpan timerInterval)
	{
		StartDelay = startDelay;
		TimerInterval = timerInterval;
		_timer = new Timer(TimerCallbackAsync, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}

	private readonly AsyncLock _timerLock = new();
	public virtual async Task<bool> StartAsync()
	{
		if (disposed)
			return false;

		using (await _timerLock.LockAsync().ConfigureAwait(false))
		{
			if (IsActive)
				return false;
				//throw new InvalidOperationException($"{GetType().Name} already started.");

			if (disposed)
				return false;

			//Start timer
			var result = _timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
			IsActive = true;
			return result;
		}
	}

	public virtual async Task StopAsync()
	{
		if (disposed)
			return;

		using (await _timerLock.LockAsync().ConfigureAwait(false))
		{
			if (!IsActive)
				return;

			if (disposed)
				return;

			//Stop timer
			_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			IsActive = false;
		}
	}

	public virtual async Task RestartAsync()
	{
		if (disposed)
			return;

		using (await _timerLock.LockAsync().ConfigureAwait(false))
		{
			if (disposed)
				return;

			_timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
			IsActive = true;
		}
	}

	private readonly AsyncLock _ticksLock = new();
#pragma warning disable VSTHRD100 // Avoid async void methods
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
	private async void TimerCallbackAsync(object? state)
	{
		if (disposed)
			return;

		using (await _ticksLock.LockAsync().ConfigureAwait(false))
		{
			if (disposed)
				return;

			if (!IsActive)
				return;

			await PauseTimerAsync().ConfigureAwait(false);

			var resume = false;

			try
			{
				IsInProcess = true;
				resume = await OnTimerAsync(state).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				resume = await OnErrorAsync(state, ex).ConfigureAwait(false);
			}
			finally
			{
				IsInProcess = false;

				if (resume)
					await ResumeTimerAsync().ConfigureAwait(false);
				else
					IsActive = false;
			}
		}
	}
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
#pragma warning restore VSTHRD100 // Avoid async void methods

	protected abstract Task<bool> OnTimerAsync(object? state);
	protected abstract Task<bool> OnErrorAsync(object? state, Exception ex);

	protected virtual async Task<bool> PauseTimerAsync()
	{
		if (disposed)
			return false;

		using (await _timerLock.LockAsync().ConfigureAwait(false))
		{
			if (disposed)
				return false;

			return _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}
	}

	protected virtual async Task<bool> ResumeTimerAsync()
	{
		if (!IsActive)
			return false;

		if (disposed)
			return false;

		using (await _timerLock.LockAsync().ConfigureAwait(false))
		{
			if (!IsActive)
				return false;

			if (disposed)
				return false;

			return _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan);
		}
	}

#if NET6_0_OR_GREATER
	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _timer.DisposeAsync();
#endif

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
				_timer.Dispose();

			disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
