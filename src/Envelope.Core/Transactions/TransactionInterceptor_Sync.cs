using Envelope.Trace;

namespace Envelope.Transactions;

public partial class TransactionInterceptor
{
	public static void Execute(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionController transactionController,
		Action<ITraceInfo, ITransactionController> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally,
		bool throwOnError = true,
		bool disposeTransactionController = true)
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
			action(traceInfo, transactionController);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (transactionController.TransactionResult == TransactionResult.Commit)
				transactionController.TransactionCoordinator.Commit();
		}
		catch (Exception ex)
		{
			try
			{
				onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
			}
			catch { }

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionController.TransactionCoordinator.TryRollback(ex);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					transactionController.TransactionCoordinator.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					transactionController.Dispose();
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
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, T> action,
		string? unhandledExceptionDetail,
		Action<ITraceInfo, Exception?, string?> onError,
		Action? @finally,
		bool disposeTransactionController = true)
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
			result = action(traceInfo, transactionController);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (transactionController.TransactionResult == TransactionResult.Commit)
				transactionController.TransactionCoordinator.Commit();

				return result;
		}
		catch (Exception ex)
		{
			try
			{
				onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
			}
			catch { }

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionController.TransactionCoordinator.TryRollback(ex);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					transactionController.TransactionCoordinator.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						onError(
							traceInfo,
							rollbackEx,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					transactionController.Dispose();
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
