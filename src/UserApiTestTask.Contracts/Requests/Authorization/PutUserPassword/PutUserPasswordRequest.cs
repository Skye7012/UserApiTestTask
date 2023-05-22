namespace UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;

/// <summary>
/// Запрос на обновление пароля пользователя
/// </summary>
public class PutUserPasswordRequest
{
	/// <summary>
	/// Старый пароль
	/// </summary>
	public string OldPassword { get; set; } = default!;

	/// <summary>
	/// Новый пароль
	/// </summary>
	public string NewPassword { get; set; } = default!;
}
