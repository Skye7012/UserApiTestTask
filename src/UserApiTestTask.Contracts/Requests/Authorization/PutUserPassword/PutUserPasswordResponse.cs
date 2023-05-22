namespace UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;

/// <summary>
/// Ответ на <see cref="PutUserPasswordRequest"/>
/// </summary>
public class PutUserPasswordResponse
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
