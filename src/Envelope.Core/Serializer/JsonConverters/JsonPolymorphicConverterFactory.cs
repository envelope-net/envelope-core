#if NET6_0_OR_GREATER
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Serializer;

public class JsonPolymorphicConverterFactory : JsonConverterFactory
{
	private static readonly ConcurrentDictionary<Type, JsonConverter?> _converters = new();
	private readonly bool _createdByJsonPolymorphicConverterAttribute = false;
	private readonly Type? _returnType;

	public JsonPolymorphicConverterFactory()
	{
	}

	public JsonPolymorphicConverterFactory(bool createdByJsonPolymorphicConverterAttribute, Type? returnType)
	{
		_createdByJsonPolymorphicConverterAttribute = createdByJsonPolymorphicConverterAttribute;
		_returnType = returnType;
	}

	public override bool CanConvert(Type typeToConvert) =>
		_createdByJsonPolymorphicConverterAttribute
		|| typeToConvert.GetCustomAttribute<JsonPolymorphicSerializerAttribute>() != null;

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> _returnType == null
		? _converters.GetOrAdd(
				typeToConvert,
				type => (JsonConverter?)Activator.CreateInstance(
					typeof(JsonPolymorphicConverter<>).MakeGenericType(typeToConvert),
					BindingFlags.Instance | BindingFlags.Public,
					null,
					Array.Empty<object>(),
					null))
		: _converters.GetOrAdd(
				typeToConvert,
				type => (JsonConverter?)Activator.CreateInstance(
					typeof(JsonPolymorphicConverter<,>).MakeGenericType(typeToConvert, _returnType),
					BindingFlags.Instance | BindingFlags.Public,
					null,
					Array.Empty<object>(),
					null));
}
#endif
