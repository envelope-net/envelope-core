namespace Envelope.Observables;

/// <summary>
/// Maintains a collection of observers of the generic type
/// </summary>
/// <typeparam name="T">The observer type</typeparam>
public class ObservableConnector<T>
	where T : class, IObserver
{
	private readonly Dictionary<long, T> _observers;
	private T[] _connected;
	private long _nextId;
	private bool _isLocked;

	public ObservableConnector()
	{
		_observers = new Dictionary<long, T>();
		_connected = Array.Empty<T>();
	}

	/// <summary>
	/// The number of observers
	/// </summary>
	public int ObserversCount => _connected.Length;

	/// <summary>
	/// Connect an observer
	/// </summary>
	/// <param name="observer">The observer to add</param>
	public IConnectHandle Connect(T observer)
	{
		if (observer == null)
			throw new ArgumentNullException(nameof(observer));

		var id = Interlocked.Increment(ref _nextId);

		if (_isLocked)
			throw new NotSupportedException("Connector is locked.");

		lock (_observers)
		{
			if (_isLocked)
				throw new NotSupportedException("Connector is locked.");

			_observers.Add(id, observer);
			_connected = _observers.Values.ToArray();
		}

		return new Handle(id, this);
	}

	public void Lock()
	{
		lock (_observers)
		{
			_isLocked = true;
		}
	}

	/// <summary>
	/// Enumerate the observers invoking the callback for each observer
	/// </summary>
	/// <param name="callback">The callback</param>
	/// <returns>An awaitable Task for the operation</returns>
	public Task ForEachAsync(Func<T, Task> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback));

		T[] connected;
		lock (_observers)
			connected = _connected;

		if (connected.Length == 0)
			return Task.CompletedTask;

		if (connected.Length == 1)
			return callback(connected[0]);

		var outputTasks = new Task[connected.Length];
		int i;
		for (i = 0; i < connected.Length; i++)
			outputTasks[i] = callback(connected[i]);

		for (i = 0; i < outputTasks.Length; i++)
		{
			if (outputTasks[i].Status != TaskStatus.RanToCompletion)
				break;
		}

		if (i == outputTasks.Length)
			return Task.CompletedTask;

		return Task.WhenAll(outputTasks);
	}

#if NET6_0_OR_GREATER
	/// <summary>
	/// Enumerate the observers invoking the callback for each observer
	/// </summary>
	/// <param name="callback">The callback</param>
	/// <returns>An awaitable ValueTask for the operation</returns>
	public async ValueTask ForEachAsync(Func<T, ValueTask> callback)
	{
		if (callback == null)
			throw new ArgumentNullException(nameof(callback));

		T[] connected;
		lock (_observers)
			connected = _connected;

		if (connected.Length == 0)
			return;

		if (connected.Length == 1)
		{
			await callback(connected[0]).ConfigureAwait(false);
			return;
		}

		for (int i = 0; i < connected.Length; i++)
			await callback(connected[i]).ConfigureAwait(false);
	}
#endif

	public void ForEach(Action<T> callback)
	{
		T[] connected;
		lock (_observers)
			connected = _connected;

		switch (connected.Length)
		{
			case 0:
				break;
			case 1:
				callback(connected[0]);
				break;
			default:
				{
					for (var i = 0; i < connected.Length; i++)
						callback(connected[i]);
					break;
				}
		}
	}

	public bool All(Func<T, bool> callback)
	{
		T[] connected;
		lock (_observers)
			connected = _connected;

		if (connected.Length == 0)
			return true;

		if (connected.Length == 1)
			return callback(connected[0]);

		for (var i = 0; i < connected.Length; i++)
		{
			if (callback(connected[i]) == false)
				return false;
		}

		return true;
	}

	void Disconnect(long id)
	{
		lock (_observers)
		{
			_observers.Remove(id);
			_connected = _observers.Values.ToArray();
		}
	}

	public void DisconnectAll()
	{
		lock (_observers)
		{
			_observers.Clear();
			_connected = _observers.Values.ToArray();
		}
	}


	private class Handle : IConnectHandle
	{
		readonly ObservableConnector<T> _observable;
		readonly long _id;

		public Handle(long id, ObservableConnector<T> observable)
		{
			_id = id;
			_observable = observable;
		}

		public void Disconnect()
		{
			_observable.Disconnect(_id);
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}
