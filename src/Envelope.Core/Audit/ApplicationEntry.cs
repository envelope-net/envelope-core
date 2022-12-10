using Envelope.Serializer;

namespace Envelope.Audit;

public class ApplicationEntry : IDictionaryObject
{
	public Guid IdApplicationEntryToken { get; set; }
	public int AuditOperation { get; set; }
	public Guid RuntimeUniqueKey { get; set; }
	public DateTime CreatedUtc { get; set; }
	public Guid? CorrelationId { get; set; }
	public string? ExternalCorrelationId { get; set; }
	public string? MainEntityIdentifier { get; set; }
	public string? Uri { get; set; }
	public Guid? IdUser { get; set; }

	public IDictionary<string, object?> ToDictionary(ISerializer? serializer = null)
	{
		var dict = new Dictionary<string, object?>
		{
			{ nameof(IdApplicationEntryToken), IdApplicationEntryToken },
			{ nameof(AuditOperation), AuditOperation },
			{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
			{ nameof(CreatedUtc), CreatedUtc },
		};

		if (CorrelationId.HasValue)
			dict.Add(nameof(CorrelationId), CorrelationId);

		if (!string.IsNullOrWhiteSpace(ExternalCorrelationId))
			dict.Add(nameof(ExternalCorrelationId), ExternalCorrelationId);

		if (!string.IsNullOrWhiteSpace(MainEntityIdentifier))
			dict.Add(nameof(MainEntityIdentifier), MainEntityIdentifier);

		if (!string.IsNullOrWhiteSpace(Uri))
			dict.Add(nameof(Uri), Uri);

		if (IdUser.HasValue)
			dict.Add(nameof(IdUser), IdUser);

		return dict;
	}

	public override string? ToString()
		=> $"{IdApplicationEntryToken} - {MainEntityIdentifier}";
}
