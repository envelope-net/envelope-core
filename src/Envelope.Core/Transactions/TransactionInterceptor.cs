using Envelope.Trace;

namespace Envelope.Transactions;

public class TransactionInterceptor
{
	protected const string UnhandledExceptionInfo = "Unhandled exception";
	protected const string DefaultRollbackErrorInfo = "Rollback error";
	protected const string DefaultFinallyInfo = "Finally error";
	protected const string DefaultDisposeInfo = "Dispose error";

	public IServiceProvider ServiceProvider { get; }
	public ITransactionManagerFactory TransactionManagerFactory { get; }
	public Func<IServiceProvider, ITransactionManager, Task<ITransactionContext>> TransactionContextFactory { get; }

	public TransactionInterceptor(
		IServiceProvider serviceProvider,
		ITransactionManagerFactory transactionManagerFactory,
		Func<IServiceProvider, ITransactionManager, Task<ITransactionContext>> transactionContextFactory)
	{
		ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		TransactionManagerFactory = transactionManagerFactory ?? throw new ArgumentNullException(nameof(transactionManagerFactory));
		TransactionContextFactory = transactionContextFactory ?? throw new ArgumentNullException(nameof(transactionContextFactory));
	}

	protected Task<ITransactionContext> CreateTransactionContextAsync()
	{
		var transactionManager = TransactionManagerFactory.Create();
		return TransactionContextFactory(ServiceProvider, transactionManager);
	}

	public virtual async Task ExecuteAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		bool throwOnError = true,
		CancellationToken cancellationToken = default)
		=> await ExecuteAsync(
				isReadOnly,
				traceInfo,
				await CreateTransactionContextAsync().ConfigureAwait(false),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				throwOnError,
				true,
				cancellationToken).ConfigureAwait(false);

	public virtual async Task<T> ExecuteAsync<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		CancellationToken cancellationToken = default)
		=> await ExecuteAsync(
				isReadOnly,
				traceInfo,
				await CreateTransactionContextAsync().ConfigureAwait(false),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true,
				cancellationToken).ConfigureAwait(false);

	public static async Task ExecuteAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		bool throwOnError = true,
		bool disposeTransactionContext = true,
		CancellationToken cancellationToken = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		if (onError == null)
			throw new ArgumentNullException(nameof(onError));

		if (transactionContext == null)
			throw new ArgumentNullException(nameof(transactionContext));

		traceInfo = TraceInfo.Create(traceInfo);

		try
		{
			await action(traceInfo, transactionContext, cancellationToken).ConfigureAwait(false);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (transactionContext.TransactionResult == TransactionResult.Commit)
				await transactionContext.TransactionManager.CommitAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			try
			{
				await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
			}
			catch { }

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionContext.TransactionManager.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
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
			if (transactionContext.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionContext.TransactionManager.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						await onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionContext)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionContext.Dispose();
#else
					await transactionContext.DisposeAsync().ConfigureAwait(false);
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
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task> onError,
		Func<Task>? @finally,
		bool disposeTransactionContext = true,
		CancellationToken cancellationToken = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		if (onError == null)
			throw new ArgumentNullException(nameof(onError));

		if (transactionContext == null)
			throw new ArgumentNullException(nameof(transactionContext));

		traceInfo = TraceInfo.Create(traceInfo);
		T? result;

		try
		{
			result = await action(traceInfo, transactionContext, cancellationToken).ConfigureAwait(false);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (transactionContext.TransactionResult == TransactionResult.Commit)
				await transactionContext.TransactionManager.CommitAsync(cancellationToken).ConfigureAwait(false);

				return result;
		}
		catch (Exception ex)
		{
			try
			{
				await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
			}
			catch { }

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionContext.TransactionManager.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
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
			if (transactionContext.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionContext.TransactionManager.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						await onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionContext)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionContext.Dispose();
#else
					await transactionContext.DisposeAsync().ConfigureAwait(false);
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
