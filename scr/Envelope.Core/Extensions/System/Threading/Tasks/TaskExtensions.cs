using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Envelope.Extensions;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
public static class TaskExtensions
{
	static readonly TimeSpan _defaultTimeout = new(0, 0, 0, 5, 0);

	public static Task OrCanceledAsync(this Task task, CancellationToken cancellationToken)
	{
		if (!cancellationToken.CanBeCanceled)
			return task;

		async Task WaitAsync()
		{
			using (RegisterTask(cancellationToken, out var cancelTask))
			{
				var completed = await Task.WhenAny(task, cancelTask).ConfigureAwait(false);
				if (completed != task)
				{
					task.IgnoreUnobservedExceptions();

					throw new OperationCanceledException(cancellationToken);
				}

				await task;
			}
		}

		return WaitAsync();
	}

	public static Task<T> OrCanceledAsync<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		if (!cancellationToken.CanBeCanceled)
			return task;

		async Task<T> WaitAsync()
		{
			using (RegisterTask(cancellationToken, out var cancelTask))
			{
				var completed = await Task.WhenAny(task, cancelTask).ConfigureAwait(false);
				if (completed != task)
				{
					task.IgnoreUnobservedExceptions();

					throw new OperationCanceledException(cancellationToken);
				}

				return await task;
			}
		}

		return WaitAsync();
	}

	public static Task OrTimeoutAsync(this Task task, int ms = 0, int s = 0, int m = 0, int h = 0, int d = 0, CancellationToken cancellationToken = default,
		[CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null)
	{
		var timeout = new TimeSpan(d, h, m, s, ms);
		if (timeout == TimeSpan.Zero)
			timeout = _defaultTimeout;

		return OrTimeoutInternalAsync(task, timeout, memberName, filePath, lineNumber, cancellationToken);
	}

	public static Task OrTimeoutAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default,
		[CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null)
	{
		return OrTimeoutInternalAsync(task, timeout, memberName, filePath, lineNumber, cancellationToken);
	}

	private static Task OrTimeoutInternalAsync(
		this Task task,
		TimeSpan timeout,
		string? memberName,
		string? filePath,
		int? lineNumber,
		CancellationToken cancellationToken)
	{
		if (task.IsCompleted)
			return task;

		async Task WaitAsync()
		{
			var cancel = new CancellationTokenSource();

			var registration = RegisterIfCanBeCanceled(cancel, cancellationToken);
			try
			{
				var delayTask = Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancel.Token);

				var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
				if (completed == delayTask)
				{
					task.IgnoreUnobservedExceptions();

					throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));
				}

				await task;
			}
			finally
			{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				registration.Dispose();
#else
				await registration.DisposeAsync();
#endif
				cancel.Cancel();
			}
		}

		return WaitAsync();
	}

	public static Task<T> OrTimeoutAsync<T>(this Task<T> task, int ms = 0, int s = 0, int m = 0, int h = 0, int d = 0,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null)
	{
		var timeout = new TimeSpan(d, h, m, s, ms);
		if (timeout == TimeSpan.Zero)
			timeout = _defaultTimeout;

		return OrTimeoutInternalAsync(task, timeout, memberName, filePath, lineNumber, cancellationToken);
	}

	public static Task<T> OrTimeoutAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default,
		[CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null)
	{
		return OrTimeoutInternalAsync(task, timeout, memberName, filePath, lineNumber, cancellationToken);
	}

	private static Task<T> OrTimeoutInternalAsync<T>(this Task<T> task, TimeSpan timeout, string? memberName, string? filePath,
		int? lineNumber, CancellationToken cancellationToken)
	{
		if (task.IsCompleted)
			return task;

		async Task<T> WaitAsync()
		{
			var cancel = new CancellationTokenSource();

			var registration = RegisterIfCanBeCanceled(cancel, cancellationToken);
			try
			{
				var delayTask = Task.Delay(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : timeout, cancel.Token);

				var completed = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
				if (completed == delayTask)
				{
					task.IgnoreUnobservedExceptions();

					throw new TimeoutException(FormatTimeoutMessage(memberName, filePath, lineNumber));
				}

				return await task;
			}
			finally
			{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				registration.Dispose();
#else
				await registration.DisposeAsync();
#endif
				cancel.Cancel();
			}
		}

		return WaitAsync();
	}

	static string FormatTimeoutMessage(string? memberName, string? filePath, int? lineNumber)
	{
		return !string.IsNullOrEmpty(memberName)
			? $"Operation in {memberName} timed out at {filePath}:{lineNumber}"
			: "Operation timed out";
	}

	/// <summary>
	/// Returns true if a Task was ran to completion (without being cancelled or faulted)
	/// </summary>
	/// <param name="task"></param>
	/// <returns></returns>
	public static bool IsCompletedSuccessfully(this Task task)
	{
		return task.Status == TaskStatus.RanToCompletion;
	}

	public static void IgnoreUnobservedExceptions(this Task task)
	{
		if (task.IsCompleted)
		{
			if (task.IsFaulted)
			{
				var _ = task.Exception;
			}

			return;
		}

#pragma warning disable VSTHRD110 // Observe result of async calls
#pragma warning disable VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
		task.ContinueWith(t =>
		{
			var _ = t.Exception;
		}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
#pragma warning restore VSTHRD110 // Observe result of async calls
	}

	/// <summary>
	/// Register a callback on the <paramref name="cancellationToken" /> which completes the resulting task.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <param name="cancelTask"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	static CancellationTokenRegistration RegisterTask(CancellationToken cancellationToken, out Task cancelTask)
	{
		if (!cancellationToken.CanBeCanceled)
			throw new ArgumentException("The cancellationToken must support cancellation", nameof(cancellationToken));

		var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		cancelTask = source.Task;

		return cancellationToken.Register(SetCompleted, source);
	}

	private static void SetCompleted(object? obj)
	{
		if (obj is TaskCompletionSource<bool> source)
			source.TrySetResult(true);
	}

	private static CancellationTokenRegistration RegisterIfCanBeCanceled(CancellationTokenSource source, CancellationToken cancellationToken)
	{
		if (cancellationToken.CanBeCanceled)
			return cancellationToken.Register(Cancel, source);

		return default;
	}

	private static void Cancel(object? obj)
	{
		if (obj is CancellationTokenSource source)
			source.Cancel();
	}
}
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
