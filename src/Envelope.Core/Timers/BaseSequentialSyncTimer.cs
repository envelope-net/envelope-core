﻿namespace Envelope.Timers;

public abstract class BaseSequentialSyncTimer : IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	protected TimeSpan StartDelay { get; set; }
	protected readonly Timer _timer;
	private bool _disposed;

	public TimeSpan TimerInterval { get; set; }
	public bool IsInProcess { get; private set; }
	public bool IsActive { get; private set; }

	public BaseSequentialSyncTimer(object? state, TimeSpan timerInterval)
		: this(state, TimeSpan.Zero, timerInterval)
	{
	}

	public BaseSequentialSyncTimer(object? state, TimeSpan startDelay, TimeSpan timerInterval)
	{
		StartDelay = startDelay;
		TimerInterval = timerInterval;
		_timer = new Timer(TimerCallback, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}

	private readonly object _timerLock = new();

	public virtual bool Start()
	{
		if (_disposed)
			return false;

		lock (_timerLock)
		{
			if (IsActive)
				return false;
				//throw new InvalidOperationException($"{GetType().Name} already started.");

			if (_disposed)
				return false;

			//Start timer
			var result = _timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
			IsActive = true;
			return result;
		}
	}

	public virtual void Stop()
	{
		if (_disposed)
			return;

		lock (_timerLock)
		{
			if (!IsActive)
				return;

			if (_disposed)
				return;

			//Stop timer
			_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			IsActive = false;
		}
	}

	public virtual void Restart()
	{
		if (_disposed)
			return;

		lock (_timerLock)
		{
			if (_disposed)
				return;

			_timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
			IsActive = true;
		}
	}

	private readonly object _ticksLock = new();
	private void TimerCallback(object? state)
	{
		if (_disposed)
			return;

		lock (_ticksLock)
		{
			if (_disposed)
				return;

			if (!IsActive)
				return;

			PauseTimer();

			var resume = false;

			try
			{
				IsInProcess = true;
				resume = OnTimer(state);
			}
			catch (Exception ex)
			{
				resume = OnError(state, ex);
			}
			finally
			{
				IsInProcess = false;

				if (resume)
					ResumeTimer();
				else
					IsActive = false;
			}
		}
	}

	protected abstract bool OnTimer(object? state);
	protected abstract bool OnError(object? state, Exception ex);
	protected virtual bool PauseTimer()
	{
		if (_disposed)
			return false;

		lock(_timerLock)
		{
			if (_disposed)
				return false;

			return _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}
	}

	protected virtual bool ResumeTimer()
	{
		if (!IsActive)
			return false;

		if (_disposed)
			return false;

		lock (_timerLock)
		{
			if (!IsActive)
				return false;

			if (_disposed)
				return false;

			return _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan);
		}
	}

#if NET6_0_OR_GREATER
	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _timer.DisposeAsync();
#endif

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
			_timer.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
