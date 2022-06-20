namespace Envelope.Trace.Dto;

public class TraceFrameDto
{
	public Guid MethodCallId { get; set; }
	public string? CallerMemberName { get; set; }
	public string? CallerFilePath { get; set; }
	public int? CallerLineNumber { get; set; }
	public IEnumerable<MethodParameter>? MethodParameters { get; set; }
	public TraceFrameDto? Previous { get; set; }

	public TraceFrameDto()
	{
	}

	public TraceFrameDto(ITraceFrame traceFrame)
	{
		if (traceFrame == null)
			throw new ArgumentNullException(nameof(traceFrame));

		MethodCallId = traceFrame.MethodCallId;
		CallerMemberName = traceFrame.CallerMemberName;
		CallerFilePath = traceFrame.CallerFilePath;
		CallerLineNumber = traceFrame.CallerLineNumber;
		MethodParameters = traceFrame.MethodParameters;
		Previous = traceFrame.Previous != null ? new TraceFrameDto(traceFrame.Previous) : null;
	}

	public TraceFrame ToTraceFrame()
	{
		var traceFrame = new TraceFrame()
		{
			MethodCallId = MethodCallId,
			CallerMemberName = CallerMemberName,
			CallerFilePath = CallerFilePath,
			CallerLineNumber = CallerLineNumber,
			MethodParameters = MethodParameters,
			Previous = Previous?.ToTraceFrame(),
		};

		return traceFrame;
	}
}
