﻿#if NET6_0_OR_GREATER
using System.Threading.Channels;

namespace Envelope.Tasks;

public class ChannelExecutor :
	IAsyncDisposable
{
	readonly Channel<IFuture> _channel;
	readonly int _concurrencyLimit;
	readonly SemaphoreSlim _limit;
	readonly Task _readerTask;
	readonly object _syncLock;

	public ChannelExecutor(int prefetchCount, int concurrencyLimit)
	{
		_concurrencyLimit = concurrencyLimit;

		var channelOptions = new BoundedChannelOptions(prefetchCount)
		{
			AllowSynchronousContinuations = true,
			FullMode = BoundedChannelFullMode.Wait,
			SingleReader = true,
			SingleWriter = false
		};

		_channel = Channel.CreateBounded<IFuture>(channelOptions);

		_syncLock = new object();
		_limit = new SemaphoreSlim(concurrencyLimit);

		_readerTask = Task.Run(() => ReadFromChannelAsync());
	}

	public ChannelExecutor(int concurrencyLimit, bool allowSynchronousContinuations = true)
	{
		_concurrencyLimit = concurrencyLimit;

		var channelOptions = new UnboundedChannelOptions
		{
			AllowSynchronousContinuations = allowSynchronousContinuations,
			SingleReader = true,
			SingleWriter = false
		};

		_channel = Channel.CreateUnbounded<IFuture>(channelOptions);

		_syncLock = new object();
		_limit = new SemaphoreSlim(concurrencyLimit);

		_readerTask = Task.Run(() => ReadFromChannelAsync());
	}

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
	public async ValueTask DisposeAsync()
	{
		_channel.Writer.Complete();

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
		await _readerTask.ConfigureAwait(false);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

		_limit.Dispose();
	}
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

	public void PushWithWait(Func<Task> method, CancellationToken cancellationToken = default)
	{
		async Task<bool> RunMethod()
		{
			await method().ConfigureAwait(false);

			return true;
		}

		var future = new Future<bool>(() => RunMethod(), cancellationToken);


		while (!cancellationToken.IsCancellationRequested)
		{
			if (_channel.Writer.TryWrite(future))
				return;

			lock (_syncLock)
				Monitor.Wait(_syncLock, 1000);
		}

		cancellationToken.ThrowIfCancellationRequested();
	}

	public async Task PushAsync(Func<Task> method, CancellationToken cancellationToken = default)
	{
		async Task<bool> RunMethod()
		{
			await method().ConfigureAwait(false);

			return true;
		}

		var future = new Future<bool>(() => RunMethod(), cancellationToken);

		await _channel.Writer.WriteAsync(future, cancellationToken).ConfigureAwait(false);
	}

	public Task RunAsync(Func<Task> method, CancellationToken cancellationToken = default)
	{
		async Task<bool> RunMethod()
		{
			await method().ConfigureAwait(false);

			return true;
		}

		return RunAsync(() => RunMethod(), cancellationToken);
	}

	public async Task<T> RunAsync<T>(Func<Task<T>> method, CancellationToken cancellationToken = default)
	{
		var future = new Future<T>(method, cancellationToken);

		await _channel.Writer.WriteAsync(future, cancellationToken).ConfigureAwait(false);

		return await future.Completed.ConfigureAwait(false);
	}

	public Task RunAsync(Action method, CancellationToken cancellationToken = default)
	{
		bool RunMethod()
		{
			method();

			return true;
		}

		return RunAsync(() => RunMethod(), cancellationToken);
	}

	public async Task<T> RunAsync<T>(Func<T> method, CancellationToken cancellationToken = default)
	{
		var future = new SynchronousFuture<T>(method, cancellationToken);

		await _channel.Writer.WriteAsync(future, cancellationToken).ConfigureAwait(false);

		return await future.Completed.ConfigureAwait(false);
	}

	async Task ReadFromChannelAsync()
	{
		try
		{
			var pending = new PendingTaskCollection(_concurrencyLimit);

			while (await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
			{
				if (!_channel.Reader.TryRead(out var future))
					continue;

				await _limit.WaitAsync().ConfigureAwait(false);

				async Task RunFuture()
				{
					var task = future.RunAsync();

					await task.ConfigureAwait(false);

					_limit.Release();

					lock (_syncLock)
						Monitor.PulseAll(_syncLock);
				}

				pending.Add(Task.Run(() => RunFuture()));
			}

			await pending.CompletedAsync().ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception)
		{
			//Logger.LogWarning(exception, "ReadFromChannel faulted");
		}
	}


	interface IFuture
	{
		Task RunAsync();
	}


	class Future<T> :
		IFuture
	{
		readonly CancellationToken _cancellationToken;
		readonly TaskCompletionSource<T> _completion;
		readonly Func<Task<T>> _method;

		public Future(Func<Task<T>> method, CancellationToken cancellationToken)
		{
			_method = method;
			_cancellationToken = cancellationToken;
			_completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
		}

		/// <summary>
		/// The post-execution result, which can be awaited
		/// </summary>
		public Task<T> Completed => _completion.Task;

		public async Task RunAsync()
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				_completion.TrySetCanceled(_cancellationToken);
				return;
			}

			try
			{
				var result = await _method().ConfigureAwait(false);

				_completion.TrySetResult(result);
			}
			catch (OperationCanceledException exception) when (exception.CancellationToken == _cancellationToken)
			{
				_completion.TrySetCanceled(exception.CancellationToken);
			}
			catch (Exception exception)
			{
				_completion.TrySetException(exception);
			}
		}
	}


	class SynchronousFuture<T> :
		IFuture
	{
		readonly CancellationToken _cancellationToken;
		readonly TaskCompletionSource<T> _completion;
		readonly Func<T> _method;

		public SynchronousFuture(Func<T> method, CancellationToken cancellationToken)
		{
			_method = method;
			_cancellationToken = cancellationToken;
			_completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
		}

		/// <summary>
		/// The post-execution result, which can be awaited
		/// </summary>
		public Task<T> Completed => _completion.Task;

		public Task RunAsync()
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				_completion.TrySetCanceled(_cancellationToken);

				return Task.CompletedTask;
			}

			try
			{
				var result = _method();

				_completion.TrySetResult(result);
			}
			catch (OperationCanceledException exception) when (exception.CancellationToken == _cancellationToken)
			{
				_completion.TrySetCanceled(exception.CancellationToken);
			}
			catch (Exception exception)
			{
				_completion.TrySetException(exception);
			}

			return Task.CompletedTask;
		}
	}
}
#endif