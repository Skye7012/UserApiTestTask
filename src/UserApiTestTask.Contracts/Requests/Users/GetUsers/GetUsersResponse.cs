using UserApiTestTask.Contracts.Requests.Common;
using UserApiTestTask.Contracts.Requests.Users.GetUser;

namespace UserApiTestTask.Contracts.Requests.Users.GetUsers;

/// <summary>
/// Ответ на запрос получения данных о пользователей
/// </summary>
public class GetUsersResponse : BaseGetResponse<GetUserResponse>
{
}
