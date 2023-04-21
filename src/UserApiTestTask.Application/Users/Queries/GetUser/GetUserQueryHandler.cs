using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Users.GetUser;

namespace UserApiTestTask.Application.Users.Queries.GetUser;

/// <summary>
/// Обработчик для of <see cref="GetUserQuery"/>
/// </summary>
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IUserService _userService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="userService">Сервис пользовательских данных</param>
	public GetUserQueryHandler(IApplicationDbContext context, IUserService userService)
	{
		_context = context;
		_userService = userService;
	}

	/// <inheritdoc/>
	public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
	{
		var user = await _context.Users
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_userService.CheckUserPermissionRule(user);

		return new GetUserResponse()
		{
			Name = user.Name,
			Gender = user.Gender,
			BirthDay = user.BirthDay,
			IsActive = user.RevokedOn == null,
		};
	}
}
