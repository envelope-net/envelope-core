using Envelope.Identity;
using System.Security.Claims;

namespace Envelope.Trace;

public interface ITraceInfoBuilder<TBuilder>
	where TBuilder : ITraceInfoBuilder<TBuilder>
{
	//TBuilder Clone(
	//	ITraceFrame currentTraceFrame,
	//	ITraceInfo traceInfo);

	ITraceInfo Build();

	TBuilder RuntimeUniqueKey(Guid runtimeUniqueKey, bool force = false);

	//TBuilder TraceFrame(ITraceFrame? traceFrame, bool force = false);

	TBuilder IdUser(Guid? idUser, bool force = false);

	TBuilder Principal(ClaimsPrincipal? user, bool force = false);

	TBuilder ExternalCorrelationId(string? externalCorrelationId, bool force = false);

	TBuilder CorrelationId(Guid? correlationId, bool force = false);
}

public abstract class TraceInfoBuilderBase<TBuilder> : ITraceInfoBuilder<TBuilder>
	where TBuilder : TraceInfoBuilderBase<TBuilder>
{
	private readonly TBuilder _builder;
	protected TraceInfo _traceInfo;

	protected TraceInfoBuilderBase(string sourceSystemName, ITraceFrame currentTraceFrame, ITraceInfo? previousTraceInfo)
	{
		if (currentTraceFrame == null)
			throw new ArgumentNullException(nameof(currentTraceFrame));

		var traceFrameBuilder = new TraceFrameBuilder(previousTraceInfo?.TraceFrame)
			.CallerMemberName(currentTraceFrame.CallerMemberName)
			.CallerFilePath(currentTraceFrame.CallerFilePath)
			.CallerLineNumber(currentTraceFrame.CallerLineNumber)
			.MethodParameters(currentTraceFrame.MethodParameters);

		((ITraceFrameBuilder<TraceFrameBuilder>)traceFrameBuilder)
			.MethodCallId(currentTraceFrame.MethodCallId);

		if (previousTraceInfo == null)
		{
			_traceInfo = new TraceInfo(sourceSystemName, traceFrameBuilder.Build());
		}
		else
		{
			_traceInfo = new TraceInfo(sourceSystemName ?? previousTraceInfo.SourceSystemName, traceFrameBuilder.Build())
			{
				RuntimeUniqueKey = previousTraceInfo.RuntimeUniqueKey,
				IdUser = previousTraceInfo.IdUser,
				Principal = previousTraceInfo.Principal,
				ExternalCorrelationId = previousTraceInfo.ExternalCorrelationId,
				CorrelationId = previousTraceInfo.CorrelationId
			};
		}

		_builder = (TBuilder)this;
	}

	public ITraceInfo Build()
		=> _traceInfo;

	public TBuilder RuntimeUniqueKey(Guid runtimeUniqueKey, bool force = false)
	{
		if (force || _traceInfo.RuntimeUniqueKey == Guid.Empty)
			_traceInfo.RuntimeUniqueKey = runtimeUniqueKey;

		return _builder;
	}

	public TBuilder IdUser(Guid? idUser, bool force = false)
	{
		if (force || !_traceInfo.IdUser.HasValue)
		{
			var principalUserId = _traceInfo.Principal?.IdentityBase?.UserId;
			if (principalUserId.HasValue && !principalUserId.Value.Equals(idUser))
				throw new InvalidOperationException($"{nameof(TraceInfo)} has already set {nameof(TraceInfo.Principal)} with {nameof(TraceInfo.IdUser)} == {principalUserId}");

			_traceInfo.IdUser = idUser;
		}

		return _builder;
	}

	public TBuilder Principal(ClaimsPrincipal? principal, bool force = false)
	{
		Guid? idUser = null;

		if (force || _traceInfo.Principal == null)
		{
			if (principal is not EnvelopePrincipal EnvelopePrincipal)
				return _builder;

			idUser = EnvelopePrincipal.IdentityBase?.UserId;
			if (_traceInfo.IdUser.HasValue)
			{
				if (!_traceInfo.IdUser.Value.Equals(idUser))
					throw new InvalidOperationException($"{nameof(TraceInfo)} has already set {nameof(TraceInfo.IdUser)} to {_traceInfo.IdUser}");
			}

			_traceInfo.Principal = EnvelopePrincipal;
			return IdUser(idUser, force);
		}

		return _builder;
	}

	public TBuilder ExternalCorrelationId(string? externalCorrelationId, bool force = false)
	{
		if (force || string.IsNullOrWhiteSpace(_traceInfo.ExternalCorrelationId))
			_traceInfo.ExternalCorrelationId = externalCorrelationId;

		return _builder;
	}

	public TBuilder CorrelationId(Guid? correlationId, bool force = false)
	{
		if (force || !_traceInfo.CorrelationId.HasValue)
			_traceInfo.CorrelationId = correlationId;

		return _builder;
	}
}

public sealed class TraceInfoBuilder : TraceInfoBuilderBase<TraceInfoBuilder>
{
	//public TraceInfoBuilder(ITraceFrame currentTraceFrame, ITraceInfo? previousTraceInfo)
	//	: this(previousTraceInfo?.SourceSystemName ?? "SYSTEM", currentTraceFrame, previousTraceInfo)
	//{
	//}

	public TraceInfoBuilder(string sourceSystemName, ITraceFrame currentTraceFrame, ITraceInfo? previousTraceInfo)
		: base(sourceSystemName, currentTraceFrame, previousTraceInfo)
	{
	}

	public static implicit operator TraceInfo?(TraceInfoBuilder builder)
	{
		if (builder == null)
			return null;

		return builder._traceInfo;
	}

	//public static implicit operator TraceInfoBuilder?(TraceInfo traceInfo)
	//{
	//	if (traceInfo == null)
	//		return null;

	//	return new TraceInfoBuilder(traceInfo);
	//}
}
