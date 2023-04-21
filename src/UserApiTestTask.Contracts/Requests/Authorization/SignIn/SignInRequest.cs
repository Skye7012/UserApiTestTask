namespace UserApiTestTask.Contracts.Requests.Authorization.SignIn;

/// <summary>
/// Запрос на авторизацию пользователя
/// </summary>
public class SignInRequest
{
	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;

	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; } = default!;
}
