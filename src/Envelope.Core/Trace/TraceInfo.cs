using Envelope.Identity;
using Envelope.Infrastructure;
using System.Runtime.CompilerServices;

namespace Envelope.Trace;

public class TraceInfo : ITraceInfo
{
	public Guid RuntimeUniqueKey { get; internal set; }

	public string SourceSystemName { get; internal set; }

	public ITraceFrame TraceFrame { get; internal set; }

	public EnvelopePrincipal? Principal { get; internal set; }

	public EnvelopeIdentity? User => Principal?.IdentityBase;

	public Guid? IdUser { get; internal set; }

	public string? ExternalCorrelationId { get; internal set; }

	public Guid? CorrelationId { get; internal set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	
	internal TraceInfo()
	{
	}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	internal TraceInfo(string sourceSystemName, ITraceFrame traceFrame)
	{
		SourceSystemName = !string.IsNullOrWhiteSpace(sourceSystemName)
			? sourceSystemName
			: throw new ArgumentNullException(nameof(sourceSystemName));

		TraceFrame = traceFrame ?? throw new ArgumentNullException(nameof(traceFrame));
		RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
	}

	public override string ToString()
		=> $"{SourceSystemName} {TraceFrame}{Environment.NewLine} {nameof(RuntimeUniqueKey)} = {RuntimeUniqueKey} | {nameof(CorrelationId)} = {CorrelationId} | {nameof(IdUser)} = {IdUser}";

	public static ITraceInfo Create(
		string sourceSystemName,
		EnvelopePrincipal? principal = null,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder(
				sourceSystemName,
				new TraceFrameBuilder()
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				null)
				.Principal(principal)
				.CorrelationId(correlationId)
			.Build();

	public static ITraceInfo Create(
		string sourceSystemName,
		ITraceFrame? previousTraceFrame,
		EnvelopePrincipal? principal = null,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder(
				sourceSystemName,
				new TraceFrameBuilder(previousTraceFrame)
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				null)
				.Principal(principal)
				.CorrelationId(correlationId)
			.Build();

	public static ITraceInfo Create(
		string sourceSystemName,
		Guid? iduser,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder(
				sourceSystemName,
				new TraceFrameBuilder()
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				null)
				.IdUser(iduser)
				.CorrelationId(correlationId)
			.Build();

	public static ITraceInfo Create(
		string sourceSystemName,
		ITraceFrame? previousTraceFrame,
		Guid? iduser,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder(
				sourceSystemName,
				new TraceFrameBuilder(previousTraceFrame)
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				null)
				.IdUser(iduser)
				.CorrelationId(correlationId)
			.Build();

	public static ITraceInfo Create(
		ITraceInfo? previousTraceInfo,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder(
				sourceSystemName ?? previousTraceInfo?.SourceSystemName!,
				new TraceFrameBuilder()
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				previousTraceInfo)
			.Build();
}
