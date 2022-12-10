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

	public Dictionary<string, string?> ContextProperties { get; }

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
		ContextProperties = traceInfo.ContextProperties.ToDictionary(x => x.Key, x => x.Value);
	}

	public TraceInfo ToTraceInfo(ITraceInfo? actualizeByTraceInfo)
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
			ContextProperties = ContextProperties.ToDictionary(x => x.Key, x => x.Value)
	};

		if (actualizeByTraceInfo != null)
		{
			traceInfo.RuntimeUniqueKey = actualizeByTraceInfo.RuntimeUniqueKey;
			traceInfo.SourceSystemName = actualizeByTraceInfo.SourceSystemName;
			//traceInfo.TraceFrame = 
			traceInfo.Principal = actualizeByTraceInfo.Principal;
			traceInfo.IdUser = actualizeByTraceInfo.IdUser;
			traceInfo.ExternalCorrelationId = actualizeByTraceInfo.ExternalCorrelationId;
			traceInfo.CorrelationId = actualizeByTraceInfo.CorrelationId;
			traceInfo.ContextProperties = actualizeByTraceInfo.ContextProperties.ToDictionary(x => x.Key, x => x.Value);

			//foreach (var kvp in actualizeByTraceInfo.Properties)
			//{
			//	if (string.IsNullOrWhiteSpace(kvp.Value))
			//	{
			//		traceInfo.Properties.Remove(kvp.Key);
			//	}
			//	else
			//	{
			//		traceInfo.Properties[kvp.Key] = kvp.Value;
			//	}
			//}
		}

		return traceInfo;
	}
}
