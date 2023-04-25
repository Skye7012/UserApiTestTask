using MediatR;
using UserApiTestTask.Contracts.Requests.Authorization.Refresh;

namespace UserApiTestTask.Application.Authorization.Commands.Refresh;

/// <summary>
/// Команда для обновления токена
/// </summary>
public class RefreshTokenCommand : RefreshTokenRequest, IRequest<RefreshTokenResponse>
{
}
