﻿using Envelope.Identity;
using Envelope.Localization;
using Envelope.Trace;
using Envelope.Web;
using System.Runtime.CompilerServices;

namespace Envelope;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IApplicationContext : IApplicationResourcesProvider
{
	ITraceInfo TraceInfo { get; }
	IRequestMetadata? RequestMetadata { get; }
	IDictionary<string, object?> Items { get; }

	ITraceInfo AddTraceFrame(ITraceFrame traceFrame);

	ITraceInfo AddTraceFrame(ITraceFrame traceFrame, Guid? idUser);

	ITraceInfo AddTraceFrame(ITraceFrame traceFrame, EnvelopePrincipal? principal);

	ITraceInfo Next(
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	ITraceInfo Next(ITraceFrame traceFrame);
}