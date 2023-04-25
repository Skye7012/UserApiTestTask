using Microsoft.AspNetCore.Http;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Extensions;

namespace UserApiTestTask.Infrastructure.Services;

/// <summary>
/// Сервис авторизации
/// </summary>
public class AuthorizationService : IAuthorizationService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="httpContextAccessor">HTTP контекст запроса</param>
	public AuthorizationService(IHttpContextAccessor httpContextAccessor)
		=> _httpContextAccessor = httpContextAccessor;

	/// <inheritdoc/>
	public bool IsAuthenticated()
		=> _httpContextAccessor.HttpContext?.User.IsAuthenticated() == true;

	/// <inheritdoc/>
	public Guid GetUserId()
		=> _httpContextAccessor.HttpContext!.User.GetUserId();

	/// <inheritdoc/>
	public Guid GetUserAccountId()
		=> _httpContextAccessor.HttpContext!.User.GetUserAccountId();

	/// <inheritdoc/>
	public bool IsAdmin()
		=> _httpContextAccessor.HttpContext!.User.GetIsAdmin();

	/// <inheritdoc/>
	public void CheckUserPermissionRule(UserAccount userAccount)
	{
		if (IsAdmin())
			return;

		if (userAccount.Id != GetUserAccountId() || userAccount.RevokedOn != null)
			throw new ForbiddenProblem("Данное действие доступно только для самого пользователя, " +
				"если он активен. Или для администратора");
	}
}
