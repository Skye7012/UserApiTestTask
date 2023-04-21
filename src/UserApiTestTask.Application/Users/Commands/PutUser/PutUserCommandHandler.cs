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
	private readonly IUserService _userService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="userService">Сервис пользователя</param>
	public PutUserCommandHandler(IApplicationDbContext context, IUserService userService)
	{
		_context = context;
		_userService = userService;
	}

	/// <inheritdoc/>
	public async Task Handle(PutUserCommand request, CancellationToken cancellationToken)
	{
		var user = await _context.Users
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_userService.CheckUserPermissionRule(user);

		user.Name = request.Name;
		user.Gender = request.Gender;
		user.BirthDay = request.BirthDay;

		await _context.SaveChangesAsync(cancellationToken);
	}
}
