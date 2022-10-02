using System.IO;

namespace Envelope.Web;

public class FormFile: IFormFile
{
	public Guid? Id { get; set; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	public Stream? Content { get; set; }
	public bool ShouldSerializeContent() => false;

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	public byte[]? Data { get; set; }
	public bool ShouldSerializeData() => false;

	public string? FileName { get; set; }
	public string? ContentType { get; set; }
	public long? Length { get; set; }
	public string? Tag { get; set; }
	public string? Hash { get; set; }
	public int DbOperation { get; set; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	public bool HasContentData => Content != null || Data != null;
	public bool ShouldSerializeHasContentData() => false;

	public Stream? OpenReadStream(bool asMemoryStream = false)
	{
		Stream? stream = null;
		if (Content != null)
		{
			if (Content.CanSeek == true)
				Content.Seek(0, SeekOrigin.Begin);

			if (asMemoryStream)
			{
				var memoryStream = new MemoryStream();
				Content.CopyTo(memoryStream);
				stream = memoryStream;
			}
			else
			{
				stream = Content;
			}
		}
		else if (Data != null)
			stream = new MemoryStream(Data);

		if (stream?.CanSeek == true)
			stream.Seek(0, SeekOrigin.Begin);

		return stream;
	}

	public async Task<Stream?> OpenReadStreamAsync(bool asMemoryStream = false, CancellationToken cancellationToken = default)
	{
		Stream? stream = null;
		if (Content != null)
		{
			if (Content.CanSeek == true)
				Content.Seek(0, SeekOrigin.Begin);

			if (asMemoryStream)
			{
				var memoryStream = new MemoryStream();
				await Content.CopyToAsync(memoryStream
#if NET6_0_OR_GREATER
					, cancellationToken
#endif
					);
				stream = memoryStream;
			}
			else
			{
				stream = Content;
			}
		}
		else if (Data != null)
			stream = new MemoryStream(Data);

		if (stream?.CanSeek == true)
			stream.Seek(0, SeekOrigin.Begin);

		return stream;
	}

	public async Task CopyToAsync(Stream targetStream, CancellationToken cancellationToken = default)
	{
		if (targetStream == null)
			throw new ArgumentNullException(nameof(targetStream));

		if (Content != null)
		{
			if (Content.CanSeek == true)
				Content.Seek(0, SeekOrigin.Begin);

			await Content.CopyToAsync(targetStream
#if NET6_0_OR_GREATER
					, cancellationToken
#endif
				);
		}
		else if (Data != null)
		{
			var stream = new MemoryStream(Data);
			stream.Seek(0, SeekOrigin.Begin);
			await stream.CopyToAsync(targetStream
#if NET6_0_OR_GREATER
					, cancellationToken
#endif
				);
		}

		if (targetStream?.CanSeek == true)
			targetStream.Seek(0, SeekOrigin.Begin);
	}

	public byte[]? GetByteArray()
	{
		if (Data != null)
		{
			return Data;
		}
		else if (Content != null)
		{
			var memoryStream = new MemoryStream();
			Content.CopyTo(memoryStream);
			var result = memoryStream.ToArray();
			return result;
		}

		return null;
	}

	public byte[]? ConvertContentToData()
	{
		if (Data != null)
		{
			return Data;
		}
		else if (Content != null)
		{
			if (Content.CanSeek == true)
				Content.Seek(0, SeekOrigin.Begin);

			var memoryStream = new MemoryStream();
			Content.CopyTo(memoryStream);
			Data = memoryStream.ToArray();
			Content = null;
			return Data;
		}

		return null;
	}
}
