using MediatR;

namespace UserApiTestTask.Application.Users.Commands.RestoreUser;

/// <summary>
/// Команда на восстановление пользователя
/// </summary>
public class RestoreUserCommand : IRequest
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public RestoreUserCommand(string login)
		=> Login = login;

	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;
}
