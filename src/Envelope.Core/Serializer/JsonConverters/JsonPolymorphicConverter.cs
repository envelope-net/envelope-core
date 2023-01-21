#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Serializer;

public class JsonPolymorphicConverter<TInterface> : JsonConverter<TInterface>
{
	private readonly Type _selfType = typeof(TInterface);

	public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException($"Envelope.Serializer.{nameof(JsonPolymorphicConverter<TInterface>)}");

	public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		System.Text.Json.JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}

public class JsonPolymorphicConverter : JsonConverter<object>
{
	public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotSupportedException($"Envelope.Serializer.{nameof(JsonPolymorphicConverter)}");

	public override bool CanConvert(Type typeToConvert)
	{
		return true;
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		
		System.Text.Json.JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}
#endif
