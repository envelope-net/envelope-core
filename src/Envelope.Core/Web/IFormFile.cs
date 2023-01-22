namespace Envelope.Web;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IFormFile
{
	Guid? Id { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	Stream? Content { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	byte[]? Data { get; }

	string? FileName { get; }
	string? ContentType { get; }
	long? Length { get; }
	string? Tag { get; }
	string? Hash { get; }
	int DbOperation { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	bool HasContentData { get; }

	Stream? OpenReadStream(bool asMemoryStream = false);

	Task<Stream?> OpenReadStreamAsync(bool asMemoryStream = false, CancellationToken cancellationToken = default);

	Task CopyToAsync(Stream targetStream, CancellationToken cancellationToken = default);

	byte[]? GetByteArray();

	byte[]? ConvertContentToData();
}
