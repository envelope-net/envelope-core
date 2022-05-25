using Envelope.Trace;

namespace Envelope.Identity;

public class AuthenticatedUser<TIdentity>
	where TIdentity : struct
{
	public TIdentity UserId { get; }
	public string Login { get; }
	public string DisplayName { get; }
	public object? UserData { get; set; }
	public List<string>? Roles { get; set; }
	public List<string>? Permissions { get; set; }
	public List<TIdentity>? RoleIds { get; set; }
	public List<TIdentity>? PermissionIds { get; set; }

	public ITraceInfo<TIdentity> TraceInfo { get; }

	public string? Password { get; set; }
	public string? Salt { get; set; }
	public string? Error { get; set; }
	public string? PasswordTemporaryUrlSlug { get; set; }

	public AuthenticatedUser(TIdentity userId, string login, string? displayName, ITraceInfo<TIdentity> traceInfo)
	{
		UserId = userId;
		Login = string.IsNullOrWhiteSpace(login)
			? throw new ArgumentNullException(nameof(login))
			: login;
		DisplayName = string.IsNullOrWhiteSpace(displayName)
			? Login
			: displayName!;
		TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
	}
}

public class AnonymousUser<TIdentity> : AuthenticatedUser<TIdentity>
	where TIdentity : struct
{
	public const string AnonymousEnvelopeUserName = "AnonymousEnvelopeUser";

	public AnonymousUser(ITraceInfo<TIdentity> traceInfo, TIdentity userId = default)
		: base(userId, AnonymousEnvelopeUserName, null, traceInfo)
	{
	}
}
