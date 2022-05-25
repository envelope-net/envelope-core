using Envelope.Extensions;

namespace Envelope.Tasks;

public class PendingTaskCollection
{
	readonly IDictionary<long, Task> _tasks;
	long _nextId;

	public PendingTaskCollection(int capacity)
	{
		_tasks = new Dictionary<long, Task>(capacity);
	}

	public void Add(IEnumerable<Task> tasks)
	{
		foreach (var task in tasks)
			Add(task);
	}

	public void Add(Task task)
	{
		if (task == null)
			throw new ArgumentNullException(nameof(task));

		if (task.Status == TaskStatus.RanToCompletion)
			return;

		var id = Interlocked.Increment(ref _nextId);

		lock (_tasks)
			_tasks.Add(id, task);

#pragma warning disable VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
#pragma warning disable VSTHRD110 // Observe result of async calls
		task.ContinueWith(x => Remove(id), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore VSTHRD110 // Observe result of async calls
#pragma warning restore VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
	}

	public async Task CompletedAsync(CancellationToken cancellationToken = default)
	{
		Task[] tasks;
		do
		{
			lock (_tasks)
			{
				if (_tasks.Count == 0)
					return;

				tasks = new Task[_tasks.Count];
				_tasks.Values.CopyTo(tasks, 0);

				_tasks.Clear();
			}

			var whenAll = Task.WhenAll(tasks);

			if (cancellationToken.CanBeCanceled)
				whenAll = whenAll.OrCanceledAsync(cancellationToken);

			await whenAll.ConfigureAwait(false);
		}
		while (tasks.Length > 0);
	}

	void Remove(long id)
	{
		lock (_tasks)
			_tasks.Remove(id);
	}
}
