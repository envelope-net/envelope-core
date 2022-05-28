using Envelope.Identity;
using Envelope.Localization;
using Envelope.Trace;
using Envelope.Web;
using System.Runtime.CompilerServices;

namespace Envelope;

public class ApplicationContext<TIdentity> : IApplicationContext<TIdentity>
	where TIdentity : struct
{
	public string SourceSystemName { get; }
	public ITraceInfo<TIdentity> TraceInfo { get; private set; }
	public IApplicationResources ApplicationResources { get; }
	public IRequestMetadata? RequestMetadata { get; }
	public IDictionary<string, object?> Items { get; }

	public ApplicationContext(ITraceInfo<TIdentity> traceInfo, IApplicationResources applicationResources, IRequestMetadata? requestMetadata)
	{
		TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		SourceSystemName = traceInfo.SourceSystemName;
		ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
		RequestMetadata = requestMetadata;
		Items = new Dictionary<string, object?>();
	}

	private readonly object _lockTrace = new();
	public ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder<TIdentity>(SourceSystemName, traceFrame, TraceInfo)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame, TIdentity? idUser)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder<TIdentity>(SourceSystemName, traceFrame, TraceInfo)
				.IdUser(idUser)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo<TIdentity> AddTraceFrame(ITraceFrame traceFrame, EnvelopePrincipal<TIdentity>? principal)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder<TIdentity>(SourceSystemName, traceFrame, TraceInfo)
				.Principal(principal)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo<TIdentity> Next(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Next(new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build());

	public ITraceInfo<TIdentity> Next(ITraceFrame traceFrame)
		=> new TraceInfoBuilder<TIdentity>(SourceSystemName, traceFrame, TraceInfo)
			.Build();
}
