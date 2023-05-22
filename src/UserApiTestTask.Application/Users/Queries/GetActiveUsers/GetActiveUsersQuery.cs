using MediatR;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;

namespace UserApiTestTask.Application.Users.Queries.GetActiveUsers;

/// <summary>
/// Запрос на получение активных пользователей
/// </summary>
public class GetActiveUsersQuery : IRequest<GetUsersResponse>
{
}
