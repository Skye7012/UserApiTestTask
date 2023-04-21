namespace UserApiTestTask.Application.Common.Static;

/// <summary>
/// Кастомные политики авторизации
/// </summary>
public class CustomPolicies
{
	/// <summary>
	/// Кастомная политика авторизации для <see cref="CustomClaims.IsAdminClaimName"/>
	/// </summary>
	public const string IsAdminClaimPolicy = "IsAdmin";
}
