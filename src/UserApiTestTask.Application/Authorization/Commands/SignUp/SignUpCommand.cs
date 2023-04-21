using MediatR;
using UserApiTestTask.Contracts.Requests.Authorization.SignUp;

namespace UserApiTestTask.Application.Authorization.Commands.SignUp;

/// <summary>
/// Команда для регистрации пользователя
/// </summary>
public class SignUpCommand : SignUpRequest, IRequest<SignUpResponse>
{
}
