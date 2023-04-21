namespace UserApiTestTask.Application.Common.Static;

/// <summary>
/// Кастомные клэймы для JWT токена
/// </summary>
public class CustomClaims
{
	/// <summary>
	/// Наименование клейма "Является ли пользователь администратором"
	/// </summary>
	public const string IsAdminClaimName = "isAdmin";
}
