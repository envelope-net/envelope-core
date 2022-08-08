using Envelope.Identity;
using Envelope.Localization;
using Envelope.Trace;
using Envelope.Web;
using System.Runtime.CompilerServices;

namespace Envelope;

public class ApplicationContext : IApplicationContext
{
	public ITraceInfo TraceInfo { get; private set; }
	public IApplicationResources ApplicationResources { get; }
	public IRequestMetadata? RequestMetadata { get; }
	public IDictionary<string, object?> Items { get; }

	public ApplicationContext(ITraceInfo traceInfo, IApplicationResources applicationResources, IRequestMetadata? requestMetadata)
	{
		TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
		RequestMetadata = requestMetadata;
		Items = new Dictionary<string, object?>();
	}

	private readonly object _lockTrace = new();
	public ITraceInfo AddTraceFrame(ITraceFrame traceFrame)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder(TraceInfo.SourceSystemName, traceFrame, TraceInfo)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo AddTraceFrame(ITraceFrame traceFrame, Guid? idUser)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder(TraceInfo.SourceSystemName, traceFrame, TraceInfo)
				.IdUser(idUser)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo AddTraceFrame(ITraceFrame traceFrame, EnvelopePrincipal? principal)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		lock (_lockTrace)
		{
			TraceInfo = new TraceInfoBuilder(TraceInfo.SourceSystemName, traceFrame, TraceInfo)
				.Principal(principal)
				.Build();
		}

		return TraceInfo;
	}

	public ITraceInfo Next(
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

	public ITraceInfo Next(ITraceFrame traceFrame)
		=> new TraceInfoBuilder(TraceInfo.SourceSystemName, traceFrame, TraceInfo)
			.Build();
}
