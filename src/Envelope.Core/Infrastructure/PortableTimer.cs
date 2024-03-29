﻿using System.Diagnostics;

namespace Envelope.Infrastructure;

public class PortableTimer : IDisposable
{
	private readonly object _stateLock = new();

	private readonly Func<CancellationToken, Task> _onTick;
	private readonly CancellationTokenSource _cancel = new();
	private readonly Timer _timer;
	private readonly Action<string, object?, object?, object?>? _errorLogger;

	private bool _running;
	private bool _disposed;

	public PortableTimer(Func<CancellationToken, Task> onTick, Action<string, object?, object?, object?>? errorLogger = null) // errorLogger = Action<format, arg0, arg1, arg2>
	{
		_onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));

		[DebuggerHidden]
		[DebuggerStepThrough]
		void Tick(object? state)
			=> OnTickAsync();

		_timer = new Timer(Tick, null, Timeout.Infinite, Timeout.Infinite);
		_errorLogger = errorLogger;
	}

	public void Start(TimeSpan interval)
	{
		if (interval < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

		lock (_stateLock)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(PortableTimer));

			_timer.Change(interval, Timeout.InfiniteTimeSpan);
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
#pragma warning disable VSTHRD100 // Avoid async void methods
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
	private async void OnTickAsync()
	{
		try
		{
			lock (_stateLock)
			{
				if (_disposed)
				{
					return;
				}

				// There's a little bit of raciness here, but it's needed to support the
				// current API, which allows the tick handler to reenter and set the next interval.

				if (_running)
				{
					Monitor.Wait(_stateLock);

					if (_disposed)
					{
						return;
					}
				}

				_running = true;
			}

			if (!_cancel.Token.IsCancellationRequested)
			{
				await _onTick(_cancel.Token).ConfigureAwait(false);
			}
		}
		catch (OperationCanceledException tcx)
		{
			_errorLogger?.Invoke("The timer was canceled during invocation: {0}", tcx, null, null);
		}
		finally
		{
			lock (_stateLock)
			{
				_running = false;
				Monitor.PulseAll(_stateLock);
			}
		}
	}
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
#pragma warning restore VSTHRD100 // Avoid async void methods

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			_cancel.Cancel();

			lock (_stateLock)
			{
				if (_disposed)
				{
					return;
				}

				while (_running)
				{
					Monitor.Wait(_stateLock);
				}

				_timer.Dispose();
			}
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
