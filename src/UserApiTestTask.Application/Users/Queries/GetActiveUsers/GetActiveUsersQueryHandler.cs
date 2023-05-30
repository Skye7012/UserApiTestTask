using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Users.GetUser;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;

namespace UserApiTestTask.Application.Users.Queries.GetActiveUsers;

/// <summary>
/// Обработчик для of <see cref="GetActiveUsersQuery"/>
/// </summary>
public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, GetUsersResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public GetActiveUsersQueryHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task<GetUsersResponse> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
	{
		_authorizationService.CheckIsAdmin();

		var users = await _context.Users
			.Where(x => x.RevokedOn == null)
			.OrderBy(x => x.CreatedOn)
			.Select(x => new GetUserResponse
			{
				BirthDay = x.BirthDay,
				Gender = x.Gender,
				Name = x.Name,
				IsActive = true,
			})
			.ToListAsync(cancellationToken);

		return new GetUsersResponse()
		{
			Items = users,
			TotalCount = users.Count,
		};
	}
}
