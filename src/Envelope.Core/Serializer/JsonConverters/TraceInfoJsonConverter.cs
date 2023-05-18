#if NET6_0_OR_GREATER
using Envelope.Serializer.JsonConverters.Model;
using Envelope.Trace;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Serializer.JsonConverters;

public class TraceInfoJsonConverter : JsonConverter<ITraceInfo>
{
	private static readonly Type _traceFrameType = typeof(ITraceFrame);
	private static readonly Type _dictionaryType = typeof(Dictionary<string, string?>);

	public override void Write(Utf8JsonWriter writer, ITraceInfo value, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Read only converter");
	}

	public override ITraceInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException();
		}
		else
		{
			var stringComparison = options.PropertyNameCaseInsensitive
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;

			var traceInfo = new DeserializedTraceInfo
			{
				//MethodParameters = ?,
			};

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return traceInfo;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					string? value;
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case var name when string.Equals(name, nameof(ITraceInfo.RuntimeUniqueKey), stringComparison):
							value = reader.GetString();
							traceInfo.RuntimeUniqueKey = Guid.TryParse(value, out var runtimeUniqueKey) ? runtimeUniqueKey : runtimeUniqueKey;
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.SourceSystemName), stringComparison):
							traceInfo.SourceSystemName = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.IdUser), stringComparison):
							value = reader.GetString();
							traceInfo.IdUser = value == null
								? null
								: (Guid.TryParse(value, out var idUserGuid) ? idUserGuid : idUserGuid);
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.CorrelationId), stringComparison):
							value = reader.GetString();
							traceInfo.CorrelationId = Guid.TryParse(value, out var correlationIdGuid) ? correlationIdGuid : null;
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.ExternalCorrelationId), stringComparison):
							traceInfo.ExternalCorrelationId = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.ContextProperties), stringComparison):
							traceInfo.ContextProperties = reader.TokenType == JsonTokenType.Null
								? new Dictionary<string, string?>()
								: ((JsonConverter<Dictionary<string, string?>>)options.GetConverter(_dictionaryType)).Read(ref reader, _dictionaryType, options)! ?? new Dictionary<string, string?>();
							break;
						case var name when string.Equals(name, nameof(ITraceInfo.TraceFrame), stringComparison):
							traceInfo.TraceFrame = reader.TokenType == JsonTokenType.Null
								? null!
								: ((JsonConverter<ITraceFrame>)options.GetConverter(_traceFrameType)).Read(ref reader, _traceFrameType, options)!;
							break;
					}
				}
			}

			return default;
		}
	}
}
#endif
