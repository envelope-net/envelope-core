namespace Envelope.Threading;

/*
 USAGE:
private static readonly AsyncLock _lock = new AsyncLock();
 
using(await _lock.LockAsync())
{
	// Critical section... You can await here!
}
 */

public class AsyncLock
{
	private readonly SemaphoreSlim _semaphoreSlim;

	public AsyncLock()
	{
		_semaphoreSlim = new SemaphoreSlim(1, 1);
	}

	public async Task<LockReleaser> LockAsync()
	{
		await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
		return new LockReleaser(_semaphoreSlim);
	}

	public struct LockReleaser : IDisposable
	{
		private readonly SemaphoreSlim _semaphoreSlim;

		public LockReleaser(SemaphoreSlim semaphoreSlim)
		{
			_semaphoreSlim = semaphoreSlim;
		}

		public void Dispose()
		{
			_semaphoreSlim.Release();
		}
	}
}
