namespace UserApiTestTask.Contracts.Requests.Authorization.SignIn;

/// <summary>
/// Ответ на <see cref="SignInRequest"/>
/// </summary>
public class SignInResponse
{
	/// <summary>
	/// Токен авторизации
	/// </summary>
	public string Token { get; set; } = default!;
}
