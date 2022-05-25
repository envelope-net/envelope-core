using System.Text;

namespace Envelope.Trace;

public interface ITraceFrame
{
	Guid MethodCallId { get; }
	string? CallerMemberName { get; }
	string? CallerFilePath { get; }
	int? CallerLineNumber { get; }
	IEnumerable<MethodParameter>? MethodParameters { get; }
	ITraceFrame? Previous { get; }

	string ToTraceStringWithMethodParameters();
	StringBuilder ToTraceString();
	IReadOnlyList<ITraceFrame> GetTrace();
	IReadOnlyList<string> GetTraceMethodIdentifiers();
}
