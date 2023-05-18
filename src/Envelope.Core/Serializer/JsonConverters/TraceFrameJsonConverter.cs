#if NET6_0_OR_GREATER
using Envelope.Serializer.JsonConverters.Model;
using Envelope.Trace;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Serializer.JsonConverters;

public class TraceFrameJsonConverter : JsonConverter<ITraceFrame>
{
	private static readonly Type _traceFrameType = typeof(ITraceFrame);

	public override void Write(Utf8JsonWriter writer, ITraceFrame value, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Read only converter");
	}

	public override ITraceFrame? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

			var traceFrame = new DeserializedTraceFrame
			{
				//MethodParameters = ?,
			};

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return traceFrame;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					string? value;
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case var name when string.Equals(name, nameof(ITraceFrame.MethodCallId), stringComparison):
							value = reader.GetString();
							traceFrame.MethodCallId = Guid.TryParse(value, out var methodCallIdGuid) ? methodCallIdGuid : methodCallIdGuid;
							break;
						case var name when string.Equals(name, nameof(ITraceFrame.CallerMemberName), stringComparison):
							traceFrame.CallerMemberName = reader.GetString();
							break;
						case var name when string.Equals(name, nameof(ITraceFrame.CallerFilePath), stringComparison):
							traceFrame.CallerFilePath = reader.GetString();
							break;
						case var name when string.Equals(name, nameof(ITraceFrame.CallerLineNumber), stringComparison):
							if (reader.TokenType != JsonTokenType.Null && reader.TryGetInt32(out var callerLineNumber))
								traceFrame.CallerLineNumber = callerLineNumber;
							break;
						case var name when string.Equals(name, nameof(ITraceFrame.Previous), stringComparison):
							traceFrame.Previous = reader.TokenType == JsonTokenType.Null
								? null
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
