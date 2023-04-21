using MediatR;
using UserApiTestTask.Contracts.Requests.Users.PutUser;

namespace UserApiTestTask.Application.Users.Commands.PutUser;

/// <summary>
/// Команда на обновление данных о пользователе
/// </summary>
public class PutUserCommand : PutUserRequest, IRequest
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public PutUserCommand(string login)
		=> Login = login;

	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;
}
