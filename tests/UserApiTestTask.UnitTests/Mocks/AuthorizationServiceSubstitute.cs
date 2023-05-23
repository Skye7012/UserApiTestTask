using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using UserApiTestTask.Infrastructure.Services;
using UserApiTestTask.UnitTests.Common.Interfaces;

namespace UserApiTestTask.UnitTests.Mocks;

/// <summary>
/// Сервис авторизации для тестов
/// </summary>
public class AuthorizationServiceSubstitute : IAuthorizationService, ISubstitute<IAuthorizationService>
{
	private readonly UserAccount _userAccount = default!;
	private readonly AuthorizationService _service;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="userAccount">Аккаунт пользователя</param>
	public AuthorizationServiceSubstitute(UserAccount? userAccount = null)
	{
		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		var context = new DefaultHttpContext();

		if (userAccount == null)
		{
			httpContextAccessor.HttpContext
				.Returns(context);

			_service = new AuthorizationService(httpContextAccessor);

			return;
		}

		if (userAccount.Id == new Guid())
			throw new ApplicationProblem("Пользователь не проинициализирован");

		if (userAccount.User == null)
			throw new NotIncludedProblem(nameof(UserAccount.User));

		_userAccount = userAccount;

		var claims = new List<Claim>
		{
			new Claim(CustomClaims.UserIdСlaimName, userAccount.User!.Id.ToString()),
			new Claim(CustomClaims.UserAccountIdClaimName, userAccount.Id.ToString()),
			new Claim(CustomClaims.IsAdminClaimName, userAccount.User!.IsAdmin.ToString()),
		};

		context.User.AddIdentity(new ClaimsIdentity(claims));
		httpContextAccessor.HttpContext
			.Returns(context);

		_service = new AuthorizationService(httpContextAccessor);
	}

	/// <inheritdoc/>
	public IAuthorizationService Create()
		=> Substitute.ForPartsOf<AuthorizationServiceSubstitute>(_userAccount);

	/// <inheritdoc/>
	public virtual bool IsAuthenticated()
		=> _service.IsAuthenticated();

	/// <inheritdoc/>
	public virtual Guid GetUserId()
		=> _service.GetUserId();

	/// <inheritdoc/>
	public Guid GetUserAccountId()
		=> _service.GetUserAccountId();

	/// <inheritdoc/>
	public virtual bool IsAdmin()
		=> _service.IsAdmin();

	/// <inheritdoc/>
	public virtual void CheckUserPermissionRule(UserAccount userAccount)
		=> _service.CheckUserPermissionRule(userAccount);
}
