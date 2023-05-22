using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;

namespace UserApiTestTask.Application.Users.Commands.PutUser;

/// <summary>
/// Обработчик для <see cref="PutUserCommand"/>
/// </summary>
public class PutUserCommandHandler : IRequestHandler<PutUserCommand>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public PutUserCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task Handle(PutUserCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_authorizationService.CheckUserPermissionRule(userAccount);

		userAccount.User!.Name = request.Name;
		userAccount.User!.Gender = request.Gender;
		userAccount.User!.BirthDay = request.BirthDay;

		await _context.SaveChangesAsync(cancellationToken);
	}
}
