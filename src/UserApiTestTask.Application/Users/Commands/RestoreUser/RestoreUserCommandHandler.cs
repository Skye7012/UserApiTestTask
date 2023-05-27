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

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	public RestoreUserCommandHandler(IApplicationDbContext context)
		=> _context = context;

	/// <inheritdoc/>
	public async Task Handle(RestoreUserCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		userAccount.Restore();

		await _context.SaveChangesAsync(cancellationToken);
	}
}
