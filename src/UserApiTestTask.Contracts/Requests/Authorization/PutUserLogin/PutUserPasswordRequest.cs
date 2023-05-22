namespace UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;

/// <summary>
/// Запрос на обновление логина пользователя
/// </summary>
public class PutUserLoginRequest
{
	/// <summary>
	/// Новый логин
	/// </summary>
	public string NewLogin { get; set; } = default!;
}
