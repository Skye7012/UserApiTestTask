using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;

namespace UserApiTestTask.Application.Users.Commands.DeleteUser;

/// <summary>
/// Обработчик для <see cref="DeleteUserCommand"/>
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public DeleteUserCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		_authorizationService.CheckIsAdmin();

		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.Include(x => x.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_context.UserAccounts.Remove(userAccount);
		await _context.SaveChangesAsync(request.WithSoftDelete, true, cancellationToken);
	}
}
