using MediatR;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;

namespace UserApiTestTask.Application.Authorization.Commands.SignIn;

/// <summary>
/// Команда для авторизации пользователя
/// </summary>
public class SignInCommand : SignInRequest, IRequest<SignInResponse>
{
}
