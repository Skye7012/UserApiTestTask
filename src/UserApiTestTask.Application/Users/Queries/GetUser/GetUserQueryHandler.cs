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
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис пользовательских данных</param>
	public GetUserQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_authorizationService.CheckUserPermissionRule(userAccount);

		return new GetUserResponse()
		{
			Name = userAccount.User!.Name,
			Gender = userAccount.User!.Gender,
			BirthDay = userAccount.User!.BirthDay,
			IsActive = userAccount.User!.RevokedOn == null,
		};
	}
}
