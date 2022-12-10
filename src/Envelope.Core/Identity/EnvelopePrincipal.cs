using System.Security.Claims;
using System.Security.Principal;

namespace Envelope.Identity;

public class EnvelopePrincipal : ClaimsPrincipal
{
	public EnvelopeIdentity? IdentityBase => Identity as EnvelopeIdentity;

	public EnvelopePrincipal()
		: base()
	{
	}

	public EnvelopePrincipal(IEnumerable<ClaimsIdentity> identities)
		: base(identities)
	{
	}

	public EnvelopePrincipal(BinaryReader reader)
		: base(reader)
	{
	}

	public EnvelopePrincipal(IIdentity identity)
		: base(identity)
	{
	}

	public EnvelopePrincipal(IPrincipal principal)
		: base(principal)
	{
	}

	public override bool IsInRole(string role)
	{
		return IdentityBase != null && IdentityBase.IsInRole(role);
	}

	public bool IsInRole(Guid role)
	{
		return IdentityBase != null && IdentityBase.IsInRole(role);
	}

	public bool IsInAllRoles(params string[] roles)
	{
		return IdentityBase != null && IdentityBase.IsInAllRoles(roles);
	}

	public bool IsInAllRoles(params Guid[] roles)
	{
		return IdentityBase != null && IdentityBase.IsInAllRoles(roles);
	}

	public bool IsInAnyRole(params string[] roles)
	{
		return IdentityBase != null && IdentityBase.IsInAnyRole(roles);
	}

	public bool IsInAnyRole(params Guid[] roles)
	{
		return IdentityBase != null && IdentityBase.IsInAnyRole(roles);
	}

	public bool HasPermission(string permission)
	{
		return IdentityBase != null && IdentityBase.HasPermission(permission);
	}

	public bool HasPermission(Guid permission)
	{
		return IdentityBase != null && IdentityBase.HasPermission(permission);
	}

	public bool HasAllPermissions(params string[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAllPermissions(permissions);
	}

	public bool HasAllPermissions(params Guid[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAllPermissions(permissions);
	}

	public bool HasAnyPermission(params string[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAnyPermission(permissions);
	}

	public bool HasAnyPermission(params Guid[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAnyPermission(permissions);
	}

	public bool HasPermissionClaim(string permission)
	{
		return IdentityBase != null && IdentityBase.HasPermissionClaim(permission);
	}

	public bool HasAllPermissionClaims(params string[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAllPermissionClaims(permissions);
	}

	public bool HasAnyPermissionClaim(params string[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAnyPermissionClaim(permissions);
	}

	public bool HasPermissionClaim(Guid permission)
	{
		return IdentityBase != null && IdentityBase.HasPermissionClaim(permission);
	}

	public bool HasAllPermissionClaims(params Guid[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAllPermissionClaims(permissions);
	}

	public bool HasAnyPermissionClaim(params Guid[] permissions)
	{
		return IdentityBase != null && IdentityBase.HasAnyPermissionClaim(permissions);
	}

	public void AddClaim(Claim claim)
	{
		if (IdentityBase == null)
			throw new InvalidOperationException($"{nameof(IdentityBase)} == null");

		IdentityBase.AddClaim(claim);
	}

	public void AddClaims(IEnumerable<Claim> claims)
	{
		if (IdentityBase == null)
			throw new InvalidOperationException($"{nameof(IdentityBase)} == null");

		IdentityBase.AddClaims(claims);
	}

	public Claim? FindFirstClaim(string type, string value)
	{
		return IdentityBase?.FindFirstClaim(type, value);
	}

	public bool HasClaim(string type)
	{
		return IdentityBase != null && IdentityBase.HasClaim(type);
	}

	public IEnumerable<Claim?>? FindAllEnvelopeClaims(string type)
	{
		return IdentityBase?.FindAllEnvelopeClaims(type);
	}

	public virtual IEnumerable<Claim>? FindAllEnvelopeClaims(Predicate<Claim> match)
	{
		return IdentityBase?.FindAllEnvelopeClaims(match);
	}

	public virtual Claim? FindFirstEnvelopeClaim(Predicate<Claim> match)
	{
		return IdentityBase?.FindFirstEnvelopeClaim(match);
	}

	public virtual Claim? FindFirstEnvelopeClaim(string type)
	{
		return IdentityBase?.FindFirstEnvelopeClaim(type);
	}

	public virtual Claim? FindFirstEnvelopeClaim(string type, string value)
	{
		return IdentityBase?.FindFirstEnvelopeClaim(type, value);
	}

	public virtual bool HasEnvelopeClaim(string type)
	{
		return IdentityBase != null && IdentityBase.HasEnvelopeClaim(type);
	}

	public virtual bool HasEnvelopeClaim(string type, string value)
	{
		return IdentityBase != null && IdentityBase.HasEnvelopeClaim(type, value);
	}

	public virtual bool HasEnvelopeClaim(Predicate<Claim> match)
	{
		return IdentityBase != null && IdentityBase.HasEnvelopeClaim(match);
	}

	public static EnvelopePrincipal? Create(string authenticationSchemeType, AuthenticatedUser authenticatedUser)
	{
		if (string.IsNullOrWhiteSpace(authenticationSchemeType))
			throw new ArgumentNullException(nameof(authenticationSchemeType));

		if (authenticatedUser == null)
			throw new ArgumentNullException(nameof(authenticatedUser));

		var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
		claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, authenticatedUser.Login));
		return Create(claimsIdentity, authenticatedUser, true, true);
	}

	public static EnvelopePrincipal? Create(
		IIdentity? identity,
		AuthenticatedUser? authenticatedUser,
		bool rolesToClams,
		bool permissionsToClaims)
	{
		if (identity == null || authenticatedUser == null)
			return null;

		var envelopeIdentity = new EnvelopeIdentity(
			identity,
			authenticatedUser.UserId,
			authenticatedUser.Login,
			authenticatedUser.DisplayName,
			authenticatedUser.UserData,
			authenticatedUser.IsSuperAdmin,
			authenticatedUser.Roles,
			authenticatedUser.RoleIds,
			authenticatedUser.Permissions,
			authenticatedUser.PermissionIds,
			rolesToClams,
			permissionsToClaims);

		var EnvelopePrincipal = new EnvelopePrincipal(envelopeIdentity);
		return EnvelopePrincipal;
	}
}
