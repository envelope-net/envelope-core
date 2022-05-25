using Envelope.Identity;
using Envelope.Web;

namespace Envelope.AspNetCore.Authentication;

public interface IAuthenticationManager<TIdentity>
	where TIdentity : struct
{
	bool LogRequestAuthentication { get; }
	bool LogRoles { get; }
	bool LogPermissions { get; }
	TIdentity? StaticUserId { get; }

	Task<AuthenticatedUser<TIdentity>?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser<TIdentity>?> CreateFromLoginPasswordAsync(string? login, string? password);

	Task<AuthenticatedUser<TIdentity>?> CreateFromLoginAsync(string? login, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser<TIdentity>?> CreateFromUserIdAsync(TIdentity? idUser, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser<TIdentity>?> CreateFromRequestAsync(IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser<TIdentity>?> SetUserDataRolesPremissionsAsync(AuthenticatedUser<TIdentity> user, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser<TIdentity>?> SetUserDataAsync(AuthenticatedUser<TIdentity> user, IRequestMetadata? requestMetadata = null, List<TIdentity>? roleIds = null);
}
