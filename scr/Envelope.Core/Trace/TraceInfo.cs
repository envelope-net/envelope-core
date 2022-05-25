using Envelope.Identity;
using Envelope.Infrastructure;
using System.Runtime.CompilerServices;

namespace Envelope.Trace;

public class TraceInfo<TIdentity> : ITraceInfo<TIdentity>
	where TIdentity : struct
{
	public Guid RuntimeUniqueKey { get; internal set; }

	public string SourceSystemName { get; internal set; }

	public ITraceFrame TraceFrame { get; }

	public EnvelopePrincipal<TIdentity>? Principal { get; internal set; }

	public EnvelopeIdentity<TIdentity>? User => Principal?.IdentityBase;

	public TIdentity? IdUser { get; internal set; }

	public string? ExternalCorrelationId { get; internal set; }

	public Guid? CorrelationId { get; internal set; }

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

	public static ITraceInfo<TIdentity> Create(
		string sourceSystemName,
		EnvelopePrincipal<TIdentity>? principal = null,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder<TIdentity>(
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

	public static ITraceInfo<TIdentity> Create(
		string sourceSystemName,
		ITraceFrame? previousTraceFrame,
		EnvelopePrincipal<TIdentity>? principal = null,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder<TIdentity>(
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

	public static ITraceInfo<TIdentity> Create(
		string sourceSystemName,
		TIdentity? iduser,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder<TIdentity>(
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

	public static ITraceInfo<TIdentity> Create(
		string sourceSystemName,
		ITraceFrame? previousTraceFrame,
		TIdentity? iduser,
		Guid? correlationId = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder<TIdentity>(
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

	public static ITraceInfo<TIdentity> Create(
		ITraceInfo<TIdentity>? previousTraceInfo,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> new TraceInfoBuilder<TIdentity>(
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
