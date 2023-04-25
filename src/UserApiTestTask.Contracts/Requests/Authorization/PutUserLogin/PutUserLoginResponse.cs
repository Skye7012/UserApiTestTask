namespace UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;

/// <summary>
/// Ответ на <see cref="PutUserLoginRequest"/>
/// </summary>
public class PutUserLoginResponse
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
