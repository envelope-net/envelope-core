#if NETSTANDARD2_0 || NETSTANDARD2_1
using System;
using System.Text;
using Newtonsoft.Json;

namespace Envelope.Serializer
{
	public class JsonSerializer : SerializerBase, ISerializer
	{
		private readonly JsonSerializerSettings? _jsonSerializerOptions;

		public JsonSerializer()
			: this(null, UTF8Encoding)
		{
		}

		public JsonSerializer(JsonSerializerSettings? jsonSerializerOptions, Encoding encoding)
			: base(encoding)
		{
			_jsonSerializerOptions = jsonSerializerOptions;
		}

		public override string SerializeAsString<T>(T value)
		{
			object? objectValue = value;
			var jsonPayload = JsonConvert.SerializeObject(objectValue, _jsonSerializerOptions);
			return jsonPayload;
		}

		public override T Deserialize<T>(string payload)
		{
			var value = JsonConvert.DeserializeObject<T>(payload, _jsonSerializerOptions);
			return value!;
		}

		public override object? Deserialize(Type returnType, string payload)
		{
			var value = JsonConvert.SerializeObject(payload, returnType, _jsonSerializerOptions);
			return value;
		}
	}
}
#endif
