using Envelope.Infrastructure;

namespace Envelope.Web.Logging;

public class ResponseDto : Serializer.IDictionaryObject
{
	public Guid RuntimeUniqueKey { get; set; }
	public DateTimeOffset CreatedUtc { get; set; }
	public Guid? CorrelationId { get; set; }
	public string? ExternalCorrelationId { get; set; }
	public int? StatusCode { get; set; }
	public string? Headers { get; set; }
	public string? ContentType { get; set; }
	public string? Body { get; set; }
	public byte[]? BodyByteArray { get; set; }
	public string? Error { get; set; }
	public decimal? ElapsedMilliseconds { get; set; }

	public ResponseDto()
	{
		RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
		CreatedUtc = DateTimeOffset.UtcNow;
	}

	public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
	{
		var dict = new Dictionary<string, object?>
		{
			{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
			{ nameof(CreatedUtc), CreatedUtc },
		};

		if (CorrelationId.HasValue)
			dict.Add(nameof(CorrelationId), CorrelationId);

		if (!string.IsNullOrWhiteSpace(ExternalCorrelationId))
			dict.Add(nameof(ExternalCorrelationId), ExternalCorrelationId);

		if (StatusCode.HasValue)
			dict.Add(nameof(StatusCode), StatusCode);

		if (!string.IsNullOrWhiteSpace(Headers))
			dict.Add(nameof(Headers), Headers);

		if (!string.IsNullOrWhiteSpace(ContentType))
			dict.Add(nameof(ContentType), ContentType);

		if (!string.IsNullOrWhiteSpace(Body))
			dict.Add(nameof(Body), Body);

		if (BodyByteArray != null)
			dict.Add(nameof(BodyByteArray), BodyByteArray);

		if (!string.IsNullOrWhiteSpace(Error))
			dict.Add(nameof(Error), Error);

		if (ElapsedMilliseconds.HasValue)
			dict.Add(nameof(ElapsedMilliseconds), ElapsedMilliseconds);

		return dict;
	}
}
