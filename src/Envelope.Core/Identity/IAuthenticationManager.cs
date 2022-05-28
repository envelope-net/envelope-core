using Envelope.Identity;
using Envelope.Web;

namespace Envelope.AspNetCore.Authentication;

public interface IAuthenticationManager
{
	bool LogRequestAuthentication { get; }
	bool LogRoles { get; }
	bool LogPermissions { get; }
	Guid? StaticUserId { get; }

	Task<AuthenticatedUser?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser?> CreateFromLoginPasswordAsync(string? login, string? password);

	Task<AuthenticatedUser?> CreateFromLoginAsync(string? login, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser?> CreateFromUserIdAsync(Guid? idUser, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser?> CreateFromRequestAsync(IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser?> SetUserDataRolesPremissionsAsync(AuthenticatedUser user, IRequestMetadata? requestMetadata = null);

	Task<AuthenticatedUser?> SetUserDataAsync(AuthenticatedUser user, IRequestMetadata? requestMetadata = null, List<Guid>? roleIds = null);
}
