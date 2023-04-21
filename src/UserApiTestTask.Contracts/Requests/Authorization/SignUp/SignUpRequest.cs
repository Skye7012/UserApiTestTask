using UserApiTestTask.Contracts.Common.Enums;

namespace UserApiTestTask.Contracts.Requests.Authorization.SignUp;

/// <summary>
/// Запрос для регистрации пользователя
/// </summary>
public class SignUpRequest
{
	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;

	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; } = default!;

	/// <summary>
	/// Имя
	/// </summary>
	public string Name { get; set; } = default!;

	/// <summary>
	/// Пол
	/// </summary>
	public Gender Gender { get; set; } = default!;

	/// <summary>
	/// Дата рождения
	/// </summary>
	public DateTime? BirthDay { get; set; }

	/// <summary>
	/// Является ли пользователь администратором
	/// </summary>
	/// <example>false</example>
	public bool IsAdmin { get; set; }
}
