#if NET6_0_OR_GREATER
using Envelope.Serializer.JsonConverters;
using System.Text.Json;

namespace Envelope.Extensions;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions AddCoreReadConverters(this JsonSerializerOptions options)
	{
		if (options == null)
			throw new ArgumentNullException(nameof(options));

		JsonConvertersConfig.AddCoreReadConverters(options.Converters);
		return options;
	}
}
#endif
