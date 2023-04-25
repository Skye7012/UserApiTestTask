using MediatR;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;

namespace UserApiTestTask.Application.Users.Queries.GetOlderThanGivenAgeUsers;

/// <summary>
/// Запрос на получение пользователей старше заданного возраста
/// </summary>
public class GetOlderThanGivenAgeUsersQuery : IRequest<GetUsersResponse>
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="age">Возраст, старше которого должны быть пользователи</param>
	public GetOlderThanGivenAgeUsersQuery(int age)
		=> Age = age;

	/// <summary>
	/// Возраст, старше которого должны быть пользователи
	/// </summary>
	public int Age { get; set; } = default!;
}
