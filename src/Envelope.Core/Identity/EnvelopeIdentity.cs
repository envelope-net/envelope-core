using Envelope.Converters;
using System.Security.Claims;
using System.Security.Principal;

namespace Envelope.Identity;

public class EnvelopeIdentity : ClaimsIdentity
{
	public const string ISSUER = "https://claims.envelope.sk";
	public const string LOGIN_CLAIM_NAME = "login";
	public const string DISPLAYNAME_CLAIM_NAME = "displayName";
	public const string USER_ID_CLAIM_NAME = "userId";
	public const string ROLE_CLAIM_NAME = "role";
	public const string ROLE_ID_CLAIM_NAME = "roleId";
	public const string PERMISSION_CLAIM_NAME = "permission";
	public const string PERMISSION_ID_CLAIM_NAME = "permissionId";

	private readonly IReadOnlyCollection<string> EMPTY_ROLES = new List<string>();
	private readonly IReadOnlyCollection<Guid> EMPTY_ROLE_IDS = new List<Guid>();
	private readonly IReadOnlyCollection<string> EMPTY_PERMISSIONS = new List<string>();
	private readonly IReadOnlyCollection<Guid> EMPTY_PERMISSIONS_IDS = new List<Guid>();

	public Guid UserId { get; }

	public string Login { get; }

	public string DisplayName { get; }

	public object? UserData { get; }

	public bool IsSuperAdmin { get; }

	public IReadOnlyCollection<string> Roles { get; }

	public IReadOnlyCollection<Guid> RoleIds { get; }

	public IReadOnlyCollection<string> Permissions { get; }

	public IReadOnlyCollection<Guid> PermissionIds { get; }

	public EnvelopeIdentity(
		string name,
		string authenticationType,
		Guid userId,
		string login,
		string displayName,
		object? userData,
		bool isSuperAdmin,
		List<string>? roles,
		List<Guid>? roleIds,
		List<string>? permissions,
		List<Guid>? permissionIds,
		bool rolesToClams,
		bool permissionsToClaims)
		: base(new GenericIdentity(
			string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name,
			string.IsNullOrWhiteSpace(authenticationType)
				? throw new ArgumentNullException(nameof(authenticationType))
				: authenticationType))
	{
		var defaulrUserId = default(Guid);
		UserId = defaulrUserId.Equals(userId)
			? throw new ArgumentNullException(nameof(userId))
			: userId;
		Login = string.IsNullOrWhiteSpace(login)
			? throw new ArgumentNullException(nameof(login))
			: login;
		DisplayName = string.IsNullOrWhiteSpace(displayName)
			? throw new ArgumentNullException(nameof(displayName))
			: displayName;
		UserData = userData;
		IsSuperAdmin = isSuperAdmin;
		Roles = roles?.AsReadOnly() ?? EMPTY_ROLES;
		RoleIds = roleIds?.AsReadOnly() ?? EMPTY_ROLE_IDS;
		Permissions = permissions?.AsReadOnly() ?? EMPTY_PERMISSIONS;
		PermissionIds = permissionIds?.AsReadOnly() ?? EMPTY_PERMISSIONS_IDS;
		AddImplicitClaims(rolesToClams, permissionsToClaims);
	}

	public EnvelopeIdentity(
		IIdentity identity,
		Guid userId,
		string login,
		string displayName,
		object? userData,
		bool isSuperAdmin,
		List<string>? roles,
		List<Guid>? roleIds,
		List<string>? permissions,
		List<Guid>? permissionIds,
		bool rolesToClams,
		bool permissionsToClaims)
		: base(identity, (identity as ClaimsIdentity)?.Claims)
	{
		//if (identity is WindowsIdentity winIdentity)
		//	WindowsIdentity = winIdentity;

		var defaulrUserId = default(Guid);
		UserId = defaulrUserId.Equals(userId)
			? throw new ArgumentNullException(nameof(userId))
			: userId;
		Login = string.IsNullOrWhiteSpace(login)
			? throw new ArgumentNullException(nameof(login))
			: login;
		DisplayName = string.IsNullOrWhiteSpace(displayName)
			? throw new ArgumentNullException(nameof(displayName))
			: displayName;
		UserData = userData;
		IsSuperAdmin = isSuperAdmin;
		Roles = roles?.AsReadOnly() ?? EMPTY_ROLES;
		RoleIds = roleIds?.AsReadOnly() ?? EMPTY_ROLE_IDS;
		Permissions = permissions?.AsReadOnly() ?? EMPTY_PERMISSIONS;
		PermissionIds = permissionIds?.AsReadOnly() ?? EMPTY_PERMISSIONS_IDS;
		AddImplicitClaims(rolesToClams, permissionsToClaims);
	}

	private void AddImplicitClaims(bool rolesToClams, bool permissionsToClaims)
	{
		AddClaim(new Claim(LOGIN_CLAIM_NAME, Login));
		AddClaim(new Claim(DISPLAYNAME_CLAIM_NAME, DisplayName));
		AddClaim(new Claim(USER_ID_CLAIM_NAME, UserId.ToString()!));

		if (rolesToClams)
		{
			AddClaims(ROLE_CLAIM_NAME, Roles);
			AddClaims(ROLE_ID_CLAIM_NAME, RoleIds);
		}
		if (permissionsToClaims)
		{
			AddClaims(PERMISSION_CLAIM_NAME, Permissions);
			AddClaims(PERMISSION_ID_CLAIM_NAME, PermissionIds);
		}
	}

	public bool IsInRole(string role)
	{
		if (string.IsNullOrWhiteSpace(role))
			throw new ArgumentNullException(nameof(role));

		return IsSuperAdmin || Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase);
	}

	public bool IsInRole(Guid role)
	{
		return IsSuperAdmin || RoleIds.Contains(role);
	}

	public bool IsInAllRoles(params string[] roles)
	{
		if (roles == null)
			throw new ArgumentNullException(nameof(roles));

		return IsSuperAdmin || roles.All(r => Roles.Contains(r, StringComparer.InvariantCultureIgnoreCase));
	}

	public bool IsInAllRoles(params Guid[] roles)
	{
		if (roles == null)
			throw new ArgumentNullException(nameof(roles));

		return IsSuperAdmin || roles.All(r => RoleIds.Contains(r));
	}

	public bool IsInAnyRole(params string[] roles)
	{
		if (roles == null)
			throw new ArgumentNullException(nameof(roles));

		return IsSuperAdmin || roles.Any(r => Roles.Contains(r, StringComparer.InvariantCultureIgnoreCase));
	}

	public bool IsInAnyRole(params Guid[] roles)
	{
		if (roles == null)
			throw new ArgumentNullException(nameof(roles));

		return IsSuperAdmin || roles.Any(r => RoleIds.Contains(r));
	}

	public bool HasPermission(string permission)
	{
		if (string.IsNullOrWhiteSpace(permission))
			throw new ArgumentNullException(nameof(permission));

		return IsSuperAdmin || Permissions.Contains(permission, StringComparer.InvariantCultureIgnoreCase);
	}

	public bool HasPermission(Guid permission)
	{
		return IsSuperAdmin || PermissionIds.Contains(permission);
	}

	public bool HasAllPermissions(params string[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.All(p => Permissions.Contains(p, StringComparer.InvariantCultureIgnoreCase));
	}

	public bool HasAllPermissions(params Guid[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.All(p => PermissionIds.Contains(p));
	}

	public bool HasAnyPermission(params string[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.Any(p => Permissions.Contains(p, StringComparer.InvariantCultureIgnoreCase));
	}

	public bool HasAnyPermission(params Guid[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.Any(r => PermissionIds.Contains(r));
	}

	public bool HasPermissionClaim(string permission)
	{
		if (string.IsNullOrWhiteSpace(permission))
			throw new ArgumentNullException(nameof(permission));

		return IsSuperAdmin || HasEnvelopeClaim(PERMISSION_CLAIM_NAME, permission);
	}

	public bool HasAllPermissionClaims(params string[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.All(permission => HasEnvelopeClaim(PERMISSION_CLAIM_NAME, permission));
	}

	public bool HasAnyPermissionClaim(params string[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.Any(permission => HasEnvelopeClaim(PERMISSION_CLAIM_NAME, permission));
	}

	public bool HasPermissionClaim(Guid permission)
	{
		return IsSuperAdmin || HasEnvelopeClaim(PERMISSION_ID_CLAIM_NAME, permission);
	}

	public bool HasAllPermissionClaims(params Guid[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.All(permission => HasEnvelopeClaim(PERMISSION_ID_CLAIM_NAME, permission));
	}

	public bool HasAnyPermissionClaim(params Guid[] permissions)
	{
		if (permissions == null)
			throw new ArgumentNullException(nameof(permissions));

		return IsSuperAdmin || permissions.Any(permission => HasEnvelopeClaim(PERMISSION_ID_CLAIM_NAME, permission));
	}

	public override void AddClaim(Claim claim)
	{
		if (claim == null)
			throw new ArgumentNullException(nameof(claim));

		base.AddClaim(new Claim(
							claim.Type,
							claim.Value,
							claim.ValueType,
							ISSUER,
							ISSUER,
							claim.Subject));
	}

	public override void AddClaims(IEnumerable<Claim?> claims)
	{
		if (claims == null)
			throw new ArgumentNullException(nameof(claims));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
		base.AddClaims(claims
			.Where(claim => claim != null)
			.Select(claim => new Claim(
									claim.Type,
									claim.Value,
									claim.ValueType,
									ISSUER,
									ISSUER,
									claim.Subject)));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}

	protected void AddClaims(string type, IEnumerable<string> values)
	{
		if (string.IsNullOrWhiteSpace(type))
			throw new ArgumentNullException(nameof(type));
		if (values == null)
			throw new ArgumentNullException(nameof(values));

		base.AddClaims(
			values
				.Select(v => new Claim(type, v, null, ISSUER)));
	}

	protected void AddClaims(string type, IEnumerable<Guid> values)
	{
		if (string.IsNullOrWhiteSpace(type))
			throw new ArgumentNullException(nameof(type));
		if (values == null)
			throw new ArgumentNullException(nameof(values));

		base.AddClaims(
			values
				.Select(v => new Claim(type, v.ToString()!, null, ISSUER)));
	}

	public Claim? FindFirstClaim(string type, string value)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
			return null;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(claim.Value, value, StringComparison.Ordinal))
				return claim;

		return null;
	}

	public bool HasClaim(string type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type))
			return false;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
				return true;

		return false;
	}

	public static bool IsEnvelopeClaim(Claim claim)
	{
		if (claim == null)
			throw new ArgumentNullException(nameof(claim));

		return
			string.Equals(claim.Issuer, ISSUER, StringComparison.OrdinalIgnoreCase)
			&& string.Equals(claim.OriginalIssuer, ISSUER, StringComparison.OrdinalIgnoreCase);
	}

	public IEnumerable<Claim?> FindAllEnvelopeClaims(string type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type))
			yield return null;
		else
			foreach (Claim claim in Claims)
				if (claim != null)
					if (string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase) && IsEnvelopeClaim(claim))
						yield return claim;
	}

	public IEnumerable<Claim> FindAllEnvelopeClaims(Predicate<Claim> match)
	{
		if (match == null)
			throw new ArgumentNullException(nameof(match));

		foreach (Claim claim in Claims)
			if (match(claim) && IsEnvelopeClaim(claim))
				yield return claim;
	}

	public Claim? FindFirstEnvelopeClaim(Predicate<Claim> match)
	{
		if (match == null)
			throw new ArgumentNullException(nameof(match));

		foreach (Claim claim in Claims)
			if (match(claim) && IsEnvelopeClaim(claim))
				return claim;

		return null;
	}

	public Claim? FindFirstEnvelopeClaim(string type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type))
			return null;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& IsEnvelopeClaim(claim))
				return claim;

		return null;
	}

	public Claim? FindFirstEnvelopeClaim(string type, string value)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
			return null;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(claim.Value, value, StringComparison.Ordinal)
					&& IsEnvelopeClaim(claim))
				return claim;

		return null;
	}

	public bool HasEnvelopeClaim(string type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type))
			return false;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& IsEnvelopeClaim(claim))
				return true;

		return false;
	}

	public bool HasEnvelopeClaim(string type, string value)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
			return false;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(claim.Value, value, StringComparison.Ordinal)
					&& IsEnvelopeClaim(claim))
				return true;

		return false;
	}

	public bool HasEnvelopeClaim(string type, Guid value)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (string.IsNullOrWhiteSpace(type))
			return false;

		foreach (Claim claim in Claims)
			if (claim != null
					&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
					&& ConverterHelper.TryConvertFrom(claim.Value, out Guid GuidValue) && GuidValue.Equals(value)
					&& IsEnvelopeClaim(claim))
				return true;

		return false;
	}

	public bool HasEnvelopeClaim(Predicate<Claim> match)
	{
		if (match == null)
			throw new ArgumentNullException(nameof(match));

		foreach (Claim claim in Claims)
			if (match(claim) && IsEnvelopeClaim(claim))
				return true;

		return false;
	}
}
