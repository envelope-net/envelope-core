using Envelope.Identity;
using Envelope.Localization;
using Envelope.Trace;
using Envelope.Web;
using System.Runtime.CompilerServices;

namespace Envelope;

public interface IApplicationContext<TIdentity>
	where TIdentity : struct
{
	string SourceSystemName { get; }
	ITraceInfo<TIdentity> TraceInfo { get; }
	IApplicationResources ApplicationResources { get; }
	IRequestMetadata? RequestMetadata { get; }
	IDictionary<string, object?> Items { get; }

	ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame);

	ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame, TIdentity? idUser);

	ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame, EnvelopePrincipal<TIdentity>? principal);

	ITraceInfo<TIdentity> Next(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	ITraceInfo<TIdentity> Next(ITraceFrame traceFrame);
}