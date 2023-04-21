using MediatR;
using UserApiTestTask.Contracts.Requests.Users.GetUser;

namespace UserApiTestTask.Application.Users.Queries.GetUser;

/// <summary>
/// Запрос на получение данных о пользователе
/// </summary>
public class GetUserQuery : IRequest<GetUserResponse>
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	public GetUserQuery(string login)
		=> Login = login;

	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;
}
