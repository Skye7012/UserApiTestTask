namespace UserApiTestTask.Contracts.Requests.Authorization.SignUp;

/// <summary>
/// Ответ на <see cref="SignUpRequest"/>
/// </summary>
public class SignUpResponse
{
	/// <summary>
	/// Идентификатор зарегистрированного пользователя
	/// </summary>
	public Guid UserId { get; set; }
}
