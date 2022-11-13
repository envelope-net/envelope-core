using Envelope.Trace;

namespace Envelope.Identity;

public class AuthenticatedUser
{
	public Guid UserId { get; }
	public string Login { get; }
	public string DisplayName { get; }
	public object? UserData { get; set; }
	public List<string>? Roles { get; set; }
	public List<string>? Permissions { get; set; }
	public List<Guid>? RoleIds { get; set; }
	public List<Guid>? PermissionIds { get; set; }

	public ITraceInfo TraceInfo { get; }

	public string? Password { get; set; }
	public string? Salt { get; set; }
	public string? Error { get; set; }
	public string? PasswordTemporaryUrlSlug { get; set; }

	public AuthenticatedUser(Guid userId, string login, string? displayName, ITraceInfo traceInfo)
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

	public override string ToString()
	{
		return $"{nameof(UserId)} = {UserId} | {nameof(Login)} = {Login}";
	}
}

public class AnonymousUser : AuthenticatedUser
{
	public const string AnonymousEnvelopeUserName = "AnonymousEnvelopeUser";

	public AnonymousUser(ITraceInfo traceInfo, Guid userId = default)
		: base(userId, AnonymousEnvelopeUserName, null, traceInfo)
	{
	}
}
