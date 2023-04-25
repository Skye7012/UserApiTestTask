namespace UserApiTestTask.Contracts.Requests.Authorization.SignIn;

/// <summary>
/// Ответ на <see cref="SignInRequest"/>
/// </summary>
public class SignInResponse
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
