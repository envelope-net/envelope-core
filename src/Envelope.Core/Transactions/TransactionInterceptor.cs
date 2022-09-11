using Envelope.Trace;

namespace Envelope.Transactions;

public partial class TransactionInterceptor
{
	protected const string UnhandledExceptionInfo = "Unhandled exception";
	protected const string DefaultRollbackErrorInfo = "Rollback error";
	protected const string DefaultFinallyInfo = "Finally error";
	protected const string DefaultDisposeInfo = "Dispose error";

	public static async Task ExecuteAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, CancellationToken, Task> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		bool throwOnError = true,
		bool disposeTransactionController = true,  //TODO: totosa uz neda disposnut!!!!!!!!!!!!!!!!!!!!!!
		CancellationToken cancellationToken = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		if (onError == null)
			throw new ArgumentNullException(nameof(onError));

		if (transactionController == null)
			throw new ArgumentNullException(nameof(transactionController));

		traceInfo = TraceInfo.Create(traceInfo);

		try
		{
			await action(traceInfo, transactionController, cancellationToken).ConfigureAwait(false);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (transactionController.TransactionResult == TransactionResult.Commit)
				await transactionController.TransactionCoordinator.CommitAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			try
			{
				await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
			}
			catch { }

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionController.TransactionCoordinator.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					await onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo).ConfigureAwait(false);
				}
				catch { }
			}

			if (throwOnError)
				throw;
		}
		finally
		{
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionController.TransactionCoordinator.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						await onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
								: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);
					}
					catch { }
				}
			}

			if (@finally != null)
			{
				try
				{
					await @finally().ConfigureAwait(false);
				}
				catch (Exception finallyEx)
				{
					try
					{
						 await onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo).ConfigureAwait(false);
					}
					catch { }
				}
			}

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					await transactionController.DisposeAsync().ConfigureAwait(false);
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						await onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo).ConfigureAwait(false);
					}
					catch { }
				}
			}
		}
	}

	public static async Task<T> ExecuteAsync<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, CancellationToken, Task<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		bool disposeTransactionController = true,
		CancellationToken cancellationToken = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		if (onError == null)
			throw new ArgumentNullException(nameof(onError));

		if (transactionController == null)
			throw new ArgumentNullException(nameof(transactionController));

		traceInfo = TraceInfo.Create(traceInfo);
		T? result;

		try
		{
			result = await action(traceInfo, transactionController, cancellationToken).ConfigureAwait(false);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (transactionController.TransactionResult == TransactionResult.Commit)
				await transactionController.TransactionCoordinator.CommitAsync(cancellationToken).ConfigureAwait(false);

				return result;
		}
		catch (Exception ex)
		{
			try
			{
				await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
			}
			catch { }

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionController.TransactionCoordinator.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					await onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo).ConfigureAwait(false);
				}
				catch { }
			}
			
			throw;
		}
		finally
		{
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionController.TransactionCoordinator.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						await onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
								: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);
					}
					catch { }
				}
			}

			if (@finally != null)
			{
				try
				{
					await @finally().ConfigureAwait(false);
				}
				catch (Exception finallyEx)
				{
					try
					{
						await onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo).ConfigureAwait(false);
					}
					catch { }
				}
			}

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					await transactionController.DisposeAsync().ConfigureAwait(false);
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						await onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo).ConfigureAwait(false);
					}
					catch { }
				}
			}
		}
	}
}
