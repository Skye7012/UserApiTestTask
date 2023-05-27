using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Users.GetUser;
using UserApiTestTask.Contracts.Requests.Users.GetUsers;

namespace UserApiTestTask.Application.Users.Queries.GetOlderThanGivenAgeUsers;

/// <summary>
/// Обработчик для of <see cref="GetOlderThanGivenAgeUsersQuery"/>
/// </summary>
public class GetOlderThanGivenAgeUsersQueryHandler : IRequestHandler<GetOlderThanGivenAgeUsersQuery, GetUsersResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IAuthorizationService _authorizationService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="dateTimeProvider">Провайдер даты и времени</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	public GetOlderThanGivenAgeUsersQueryHandler(
		IApplicationDbContext context,
		IDateTimeProvider dateTimeProvider,
		IAuthorizationService authorizationService)
	{
		_context = context;
		_dateTimeProvider = dateTimeProvider;
		_authorizationService = authorizationService;
	}

	/// <inheritdoc/>
	public async Task<GetUsersResponse> Handle(GetOlderThanGivenAgeUsersQuery request, CancellationToken cancellationToken)
	{
		_authorizationService.CheckIsAdmin();

		var utcNow = _dateTimeProvider.UtcNow;

		var users = await _context.Users
			.Where(x =>
				x.BirthDay.HasValue
				&& request.Age <
						(((utcNow.Year * 100 + utcNow.Month) * 100 + utcNow.Day)
						- ((x.BirthDay.Value.Year * 100 + x.BirthDay.Value.Month) * 100 + x.BirthDay.Value.Day))
					/ 10000)
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
