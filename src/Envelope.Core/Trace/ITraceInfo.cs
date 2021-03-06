using Envelope.Identity;

namespace Envelope.Trace;

public interface ITraceInfo
{
	Guid RuntimeUniqueKey { get; }

	string SourceSystemName { get; }

	ITraceFrame TraceFrame { get; }

	EnvelopePrincipal? Principal { get; }

	EnvelopeIdentity? User { get; }

	Guid? IdUser { get; }

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
}
