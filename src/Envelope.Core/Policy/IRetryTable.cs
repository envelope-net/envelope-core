namespace Envelope.Policy;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IRetryTable
{
	IReadOnlyDictionary<int, TimeSpan> IterationRetryTable { get; } //Dictionary<IterationCount, TimeSpan>

	TimeSpan? DefaultRetryInterval { get; }

	bool Add(int iterationCount, TimeSpan delay, bool force = true);

	TimeSpan? GetFirstRetryTimeSpan();

	TimeSpan? GetRetryTimeSpan(int currentRetryCount);
}
