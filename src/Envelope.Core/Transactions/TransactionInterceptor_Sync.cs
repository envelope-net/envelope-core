using Envelope.Trace;

namespace Envelope.Transactions;

public partial class TransactionInterceptor
{
	public virtual void Execute(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Action<ITraceInfo, ITransactionContext> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally,
		bool throwOnError = true)
		=> Execute(
				isReadOnly,
				traceInfo,
				CreateTransactionContext(),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				throwOnError,
				true);

	public virtual T Execute<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, T> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally)
		=> Execute(
				isReadOnly,
				traceInfo,
				CreateTransactionContext(),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true);

	public static void Execute(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Action<ITraceInfo, ITransactionContext> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally,
		bool throwOnError = true,
		bool disposeTransactionContext = true)
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
			action(traceInfo, transactionContext);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (transactionContext.TransactionResult == TransactionResult.Commit)
				transactionContext.TransactionManager.Commit();
		}
		catch (Exception ex)
		{
			try
			{
				onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
			}
			catch { }

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionContext.TransactionManager.TryRollback(ex);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo);
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
					transactionContext.TransactionManager.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
								: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));
					}
					catch { }
				}
			}

			if (@finally != null)
			{
				try
				{
					@finally();
				}
				catch (Exception finallyEx)
				{
					try
					{
						 onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo);
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
					transactionContext.Dispose();
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo);
					}
					catch { }
				}
			}
		}
	}

	public static T Execute<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, T> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally,
		bool disposeTransactionContext = true)
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
			result = action(traceInfo, transactionContext);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (transactionContext.TransactionResult == TransactionResult.Commit)
				transactionContext.TransactionManager.Commit();

				return result;
		}
		catch (Exception ex)
		{
			try
			{
				onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
			}
			catch { }

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionContext.TransactionManager.TryRollback(ex);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo);
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
					transactionContext.TransactionManager.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
								: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));
					}
					catch { }
				}
			}

			if (@finally != null)
			{
				try
				{
					@finally();
				}
				catch (Exception finallyEx)
				{
					try
					{
						onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo);
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
					transactionContext.Dispose();
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo);
					}
					catch { }
				}
			}
		}
	}
}
