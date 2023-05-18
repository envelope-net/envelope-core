using System.Runtime.CompilerServices;
using System.Text;

namespace Envelope.Trace;

public class TraceFrame : ITraceFrame
{
	public Guid MethodCallId { get; internal set; } = Guid.NewGuid();
	public string? CallerMemberName { get; internal set; }
	public string? CallerFilePath { get; internal set; }
	public int? CallerLineNumber { get; internal set; }
	public IEnumerable<MethodParameter>? MethodParameters { get; internal set; }
	public ITraceFrame? Previous { get; internal set; }

	internal TraceFrame()
	{
	}

	public string ToTraceStringWithMethodParameters()
	{
		var sb = ToTraceString();

		if (MethodParameters != null)
		{
			foreach (var param in MethodParameters)
			{
				if (string.IsNullOrWhiteSpace(param.ParameterName))
				{
					sb.AppendLine();
					sb.Append($"\t-param[{param.ParameterName}]: {param.SerializedValue}");
				}
			}
		}

		return sb.ToString();
	}

	public StringBuilder ToTraceString()
	{
		var empty = true;
		var sb = new StringBuilder();

		if (!string.IsNullOrWhiteSpace(CallerFilePath))
		{
			var callerFileName = CallerFilePath!.Trim().EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)
				? $"{Directory.GetParent(CallerFilePath)?.Name}\\{Path.GetFileName(CallerFilePath)}"
				: CallerFilePath;

			sb.Append(callerFileName);
			empty = false;
		}

		if (!string.IsNullOrWhiteSpace(CallerMemberName))
		{
			if (empty)
				sb.Append(CallerMemberName);
			else
				sb.Append(" > ").Append(CallerMemberName);
		}

		if (CallerLineNumber.HasValue)
			sb.Append(" <<r.").Append(CallerLineNumber).Append(">>");

		return sb;
	}

	public IReadOnlyList<ITraceFrame> GetTrace()
	{
		var result = new List<ITraceFrame> { this };

		if (Previous == null)
			return result;

		var previous = Previous;
		while (previous != null)
		{
			result.Add(previous);
			previous = previous.Previous;
		}

		return result;
	}

	public IReadOnlyList<string> GetTraceMethodIdentifiers()
		=> GetTrace().Select(x => x.ToTraceString().ToString()).ToList();

	public override string ToString()
		=> string.Join(Environment.NewLine, GetTrace().Reverse().Select(x => x.ToTraceStringWithMethodParameters()));

	public static ITraceFrame Create(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder()
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build();

	public static ITraceFrame Create(
		ITraceFrame? previousTraceFrame,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder(previousTraceFrame)
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build();

	public static string GetThisCallerMethodFullName(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder()
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build()
			.ToTraceStringWithMethodParameters();

	public static string GetThisCallerMethodFullName(
		ITraceFrame? previousTraceFrame,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder(previousTraceFrame)
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build()
			.ToTraceStringWithMethodParameters();

	public static string GetCallerPath(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder()
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build()
			.ToString() ?? "";

	public static string GetCallerPath(
		ITraceFrame? previousTraceFrame,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceFrameBuilder(previousTraceFrame)
			.CallerMemberName(memberName)
			.CallerFilePath(sourceFilePath)
			.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
			.MethodParameters(methodParameters)
			.Build()
			.ToString() ?? "";
}
