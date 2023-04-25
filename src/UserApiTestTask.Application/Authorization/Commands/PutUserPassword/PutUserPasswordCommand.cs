using MediatR;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;

namespace UserApiTestTask.Application.Authorization.Commands.PutUserPassword;

/// <summary>
/// Команда на обновление пароля пользователя
/// </summary>
public class PutUserPasswordCommand : PutUserPasswordRequest, IRequest<PutUserPasswordResponse>
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public PutUserPasswordCommand(string login)
		=> Login = login;

	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;
}
