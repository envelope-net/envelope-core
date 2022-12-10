using Envelope.Identity;

namespace Envelope.Trace;

public interface ITraceInfo<TIdentity>
	where TIdentity : struct
{
	Guid RuntimeUniqueKey { get; }

	string SourceSystemName { get; }

	ITraceFrame TraceFrame { get; }

	EnvelopePrincipal<TIdentity>? Principal { get; }

	EnvelopeIdentity<TIdentity>? User { get; }

	TIdentity? IdUser { get; }

	/// <summary>
	/// Usualy HttpContext.TraceIdentifier, which is marked as external because can be changed
	/// by RequestCorrelationMiddleware based on Request header (DefaultHeader = "X-Correlation-ID") from client - it may be not unique
	/// or can be changed by another middleware / filter
	/// </summary>
	string? ExternalCorrelationId { get; }

	/// <summary>
	/// Usualy HttpContext.Item[X-Correlation-ID] set by RequestCorrelationMiddleware
	/// It is unique identifier for current request
	/// </summary>
	Guid? CorrelationId { get; }

	Dictionary<string, string> Properties { get; }

	ITraceInfo<TIdentity> SetProperty(string key, string? value, bool force = false);
}
