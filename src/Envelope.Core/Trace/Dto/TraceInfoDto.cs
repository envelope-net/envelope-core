using Envelope.Identity;

namespace Envelope.Trace.Dto;

public class TraceInfoDto
{
	public Guid RuntimeUniqueKey { get; set; }

	public string SourceSystemName { get; set; }

	public TraceFrameDto TraceFrame { get; set; }

	public EnvelopePrincipal? Principal { get; set; }

	public Guid? IdUser { get; set; }

	public string? ExternalCorrelationId { get; set; }

	public Guid? CorrelationId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public TraceInfoDto()
	{
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public TraceInfoDto(ITraceInfo traceInfo)
	{
		if (traceInfo == null)
			throw new ArgumentNullException(nameof(traceInfo));

		RuntimeUniqueKey = traceInfo.RuntimeUniqueKey;
		SourceSystemName = traceInfo.SourceSystemName;
		TraceFrame = new TraceFrameDto(traceInfo.TraceFrame);
		Principal = traceInfo.Principal;
		IdUser = traceInfo.IdUser;
		ExternalCorrelationId = traceInfo.ExternalCorrelationId;
		CorrelationId = traceInfo.CorrelationId;
	}

	public TraceInfo ToTraceInfo()
	{
		var traceInfo = new TraceInfo()
		{
			RuntimeUniqueKey = RuntimeUniqueKey,
			SourceSystemName = SourceSystemName,
			TraceFrame = TraceFrame.ToTraceFrame(),
			Principal = Principal,
			IdUser = IdUser,
			ExternalCorrelationId = ExternalCorrelationId,
			CorrelationId = CorrelationId,
		};

		return traceInfo;
	}
}
