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

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	public DeleteUserCommandHandler(IApplicationDbContext context)
		=> _context = context;

	/// <inheritdoc/>
	public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.Include(x => x.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_context.UserAccounts.Remove(userAccount);
		await _context.SaveChangesAsync(request.WithSoftDelete, true, cancellationToken);
	}
}
