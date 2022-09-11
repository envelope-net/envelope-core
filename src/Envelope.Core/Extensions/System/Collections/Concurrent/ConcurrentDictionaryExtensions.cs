using System.Collections.Concurrent;

namespace Envelope.Extensions;

public static class ConcurrentDictionaryExtensions
{
	public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory)
		where TKey : notnull
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));
		if (addValueFactory == null)
			throw new ArgumentNullException(nameof(addValueFactory));

		return dictionary.AddOrUpdate(
			key,
			addValueFactory,
			(existKey, oldValue) => addValueFactory(key));
	}

	public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue addValue)
		where TKey : notnull
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));

		return dictionary.AddOrUpdate(
			key,
			addValue,
			(existKey, oldValue) => addValue);
	}
	
	/// <summary>
	 /// Returns an existing task from the concurrent dictionary, or adds a new task
	 /// using the specified asynchronous factory method. Concurrent invocations for
	 /// the same key are prevented, unless the task is removed before the completion
	 /// of the delegate. Failed tasks are evicted from the concurrent dictionary.
	 /// </summary>
	public static Task<TValue> GetOrAddAsync<TKey, TValue>(
		this ConcurrentDictionary<TKey, Task<TValue>> source, TKey key,
		Func<TKey, Task<TValue>> valueFactory)
		where TKey : notnull
	{
		if (!source.TryGetValue(key, out var currentTask))
		{
			Task<TValue>? newTask = null;
			var newTaskTask = new Task<Task<TValue>>(async () =>
			{
				try { return await valueFactory(key).ConfigureAwait(false); }
				catch
				{
#if NET6_0_OR_GREATER
					source.TryRemove(KeyValuePair.Create(key, newTask)!);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
					source.TryRemove(key, out var tsk);
#endif
					throw;
				}
			});

			newTask = newTaskTask.Unwrap();
			currentTask = source.GetOrAdd(key, newTask);
			if (currentTask == newTask)
				newTaskTask.Start(TaskScheduler.Default);
		}

		return currentTask;
	}

	public static Task<TValue> GetOrAddAsync<TKey, TValue>(
		this ConcurrentDictionary<TKey, Task<TValue>> source, TKey key,
		Func<TKey, TValue> valueFactory)
		where TKey : notnull
	{
		if (!source.TryGetValue(key, out var currentTask))
		{
			Task<TValue>? newTask = null;
			newTask = new Task<TValue>(() =>
			{
				try { return valueFactory(key); }
				catch
				{
#if NET6_0_OR_GREATER
					source.TryRemove(KeyValuePair.Create(key, newTask)!);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
					source.TryRemove(key, out var tsk);
#endif
					throw;
				}
			});
			currentTask = source.GetOrAdd(key, newTask);
			if (currentTask == newTask)
				newTask.Start(TaskScheduler.Default);
		}

		return currentTask;
	}
}
