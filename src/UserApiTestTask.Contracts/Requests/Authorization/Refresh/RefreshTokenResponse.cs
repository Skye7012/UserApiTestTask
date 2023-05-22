namespace UserApiTestTask.Contracts.Requests.Authorization.Refresh;

/// <summary>
/// Ответ на <see cref="RefreshTokenRequest"/>
/// </summary>
public class RefreshTokenResponse
{
	/// <summary>
	/// Access Токен
	/// </summary>
	public string AccessToken { get; set; } = default!;

	/// <summary>
	/// Refresh Токен
	/// </summary>
	public string RefreshToken { get; set; } = default!;
}
