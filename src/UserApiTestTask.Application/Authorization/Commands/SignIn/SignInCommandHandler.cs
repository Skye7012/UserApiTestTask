using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Authorization.Commands.SignIn;

/// <summary>
/// Обработчик для <see cref="SignInCommand"/>
/// </summary>
public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IUserService _userService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="userService">Сервис пользователя</param>
	public SignInCommandHandler(IApplicationDbContext context, IUserService userService)
	{
		_context = context;
		_userService = userService;
	}

	/// <inheritdoc/>
	public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
	{
		var user = await _context.Users
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		if (!_userService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
		{
			throw new ValidationProblem("Неправильный пароль");
		}

		string token = _userService.CreateToken(user);

		return new SignInResponse()
		{
			Token = token,
		};
	}
}
