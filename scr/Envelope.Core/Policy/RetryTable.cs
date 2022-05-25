using Envelope.Extensions;

namespace Envelope.Policy;

public class RetryTable : IRetryTable
{
	public Dictionary<int, TimeSpan> IterationRetryTable { get; set; } //Dictionary<IterationCount, TimeSpan>
	IReadOnlyDictionary<int, TimeSpan> IRetryTable.IterationRetryTable => IterationRetryTable;

	public TimeSpan? DefaultRetryInterval { get; set; }

	public RetryTable()
	{
		IterationRetryTable = new Dictionary<int, TimeSpan>();
	}

	public RetryTable(Dictionary<int, TimeSpan> delayTable)
	{
		if (delayTable == null)
			throw new ArgumentNullException(nameof(delayTable));

		IterationRetryTable = new Dictionary<int, TimeSpan>(delayTable);
	}

	public bool Add(int iterationCount, TimeSpan delay, bool force = true)
	{
		if (iterationCount < 0)
			throw new ArgumentOutOfRangeException(nameof(iterationCount));

		if (force)
		{
			IterationRetryTable[iterationCount] = delay;
			return true;
		}
		else
		{
			var result = IterationRetryTable.TryAdd(iterationCount, delay);
			return result;
		}
	}

	public TimeSpan? GetFirstRetryTimeSpan()
	{
		if (IterationRetryTable.Count == 0)
			return DefaultRetryInterval;

		var minIteration = IterationRetryTable.Keys.Where(x => 0 <= x).Min();
		return IterationRetryTable[minIteration];
	}

	public TimeSpan? GetRetryTimeSpan(int currentRetryCount)
	{
		if (IterationRetryTable.Count == 0)
			return DefaultRetryInterval;

		TimeSpan? result = null;
		int? bestDelta = null;
		foreach (var retry in IterationRetryTable.Keys.Where(x => 0 <= x))
		{
			var value = IterationRetryTable[retry];
			var delta = Math.Abs(retry - currentRetryCount);
			if (bestDelta.HasValue)
			{
				if ((delta < bestDelta.Value)
					|| (delta == bestDelta.Value && value < result))
				{
					bestDelta = delta;
					result = value;
				}
			}
			else
			{
				bestDelta = delta;
				result = value;
			}
		}

		return result ?? DefaultRetryInterval;
	}
}
