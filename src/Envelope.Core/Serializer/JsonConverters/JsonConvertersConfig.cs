#if NET6_0_OR_GREATER
using Envelope.Extensions;
using System.Text.Json.Serialization;

namespace Envelope.Serializer.JsonConverters;

public static class JsonConvertersConfig
{
	public static void AddCoreReadConverters(IList<JsonConverter> converters)
	{
		if (converters == null)
			throw new ArgumentNullException(nameof(converters));

		converters.AddUniqueItem(new TraceFrameJsonConverter());
		converters.AddUniqueItem(new TraceInfoJsonConverter());
		converters.AddUniqueItem(new EnvironmentInfoJsonConverter());
	}
}
#endif
