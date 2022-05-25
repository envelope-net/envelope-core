namespace Envelope.Policy;

public interface IRetryTable
{
	IReadOnlyDictionary<int, TimeSpan> IterationRetryTable { get; } //Dictionary<IterationCount, TimeSpan>

	TimeSpan? DefaultRetryInterval { get; }

	bool Add(int iterationCount, TimeSpan delay, bool force = true);

	TimeSpan? GetFirstRetryTimeSpan();

	TimeSpan? GetRetryTimeSpan(int currentRetryCount);
}
