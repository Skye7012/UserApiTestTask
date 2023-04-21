using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Exceptions;

/// <summary>
/// Не найден пользователь
/// </summary>
public class UserNotFoundProblem : EntityNotFoundProblem<User>
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public UserNotFoundProblem(string login)
		: base($"Не удалось найти пользователя с логином = {login}")
	{ }
}
