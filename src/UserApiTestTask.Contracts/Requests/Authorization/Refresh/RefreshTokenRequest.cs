namespace UserApiTestTask.Contracts.Requests.Authorization.Refresh;

/// <summary>
/// Запрос на обновление токена
/// </summary>
public class RefreshTokenRequest
{
	/// <summary>
	/// Refresh токен
	/// </summary>
	public string RefreshToken { get; set; } = default!;
}
