using MediatR;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;

namespace UserApiTestTask.Application.Authorization.Commands.PutUserLogin;

/// <summary>
/// Команда на обновление логина пользователя
/// </summary>
public class PutUserLoginCommand : PutUserLoginRequest, IRequest<PutUserLoginResponse>
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public PutUserLoginCommand(string login)
		=> Login = login;

	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;
}
