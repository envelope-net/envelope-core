﻿using Envelope.Extensions;
using System.Runtime.ExceptionServices;

namespace Envelope.Policy.Internal;

internal static class AsyncRetryPolicyProvider
{
	internal static async Task<TResult> ExecuteAsync<TResult>(
		Func<CancellationToken, Task<TResult>> action,
		ExceptionPredicates shouldRetryExceptionPredicates,
		ResultPredicates<TResult> shouldRetryResultPredicates,
		Func<DelegateResult<TResult>, TimeSpan, int, Task<RetryResult?>> onRetryAsync,
		int permittedRetryCount = int.MaxValue,
		IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
		Func<int, DelegateResult<TResult>, TimeSpan>? sleepDurationProvider = null,
		bool continueOnCapturedContext = false,
		CancellationToken cancellationToken = default)
	{
		int tryCount = 0;
		var sleepDurationsEnumerator = sleepDurationsEnumerable?.GetEnumerator();

		try
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				bool canRetry;
				DelegateResult<TResult> delegateResult;

				try
				{
					TResult result = await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);

					if (!shouldRetryResultPredicates.AnyMatch(result))
					{
						return result;
					}

					canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerator == null || sleepDurationsEnumerator.MoveNext());

					if (!canRetry)
					{
						return result;
					}

					delegateResult = new DelegateResult<TResult>(result);
				}
				catch (Exception ex)
				{
					var handledException = shouldRetryExceptionPredicates.FirstMatchOrDefault(ex);
					if (handledException == null)
					{
						throw;
					}

					canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerator == null || sleepDurationsEnumerator.MoveNext());

					if (!canRetry)
					{
						handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
						throw;
					}

					delegateResult = new DelegateResult<TResult>(handledException);
				}

				if (tryCount < int.MaxValue) { tryCount++; }

				TimeSpan waitDuration = sleepDurationsEnumerator?.Current ?? (sleepDurationProvider?.Invoke(tryCount, delegateResult) ?? TimeSpan.Zero);

				var retryResult = await onRetryAsync(delegateResult, waitDuration, tryCount).ConfigureAwait(continueOnCapturedContext);
				if (retryResult.HasValue)
				{
					if (retryResult.Value.CanRetry)
					{
						if (retryResult.Value.WaitDuration.HasValue)
						{
							waitDuration = retryResult.Value.WaitDuration.Value;
						}
					}
					else
					{
						if (delegateResult.Exception != null)
						{
							ExceptionDispatchInfo.Capture(delegateResult.Exception).Throw();
						}
						else
						{
							return delegateResult.Result!;
						}
					}
				}

				if (waitDuration > TimeSpan.Zero)
				{
					await Task.Delay(waitDuration, cancellationToken).ConfigureAwait(continueOnCapturedContext);
				}
			}
		}
		finally
		{
			sleepDurationsEnumerator?.Dispose();
		}
	}
}
