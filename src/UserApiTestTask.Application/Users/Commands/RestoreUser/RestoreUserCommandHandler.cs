using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;

namespace UserApiTestTask.Application.Users.Commands.RestoreUser;

/// <summary>
/// Обработчик для <see cref="RestoreUserCommand"/>
/// </summary>
public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public RestoreUserCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task Handle(RestoreUserCommand request, CancellationToken cancellationToken)
	{
		_authorizationService.CheckIsAdmin();

		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		userAccount.Restore();

		await _context.SaveChangesAsync(cancellationToken);
	}
}
