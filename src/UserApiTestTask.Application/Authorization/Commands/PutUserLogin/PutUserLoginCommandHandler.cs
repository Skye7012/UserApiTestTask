using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Authorization.Commands.PutUserLogin;

/// <summary>
/// Обработчик для <see cref="PutUserLoginCommand"/>
/// </summary>
public class PutUserLoginCommandHandler : IRequestHandler<PutUserLoginCommand, PutUserLoginResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;
	private readonly ITokenService _tokenService;
	private readonly IUserAccountCacheRepository _userAccountCache;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	/// <param name="tokenService">Сервис JWT токенов</param>
	/// <param name="userAccountCache">Репозиторий кэширования аккаунта пользователя</param>
	public PutUserLoginCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService,
		ITokenService tokenService,
		IUserAccountCacheRepository userAccountCache)
	{
		_context = context;
		_authorizationService = authorizationService;
		_tokenService = tokenService;
		_userAccountCache = userAccountCache;
	}

	/// <inheritdoc/>
	public async Task<PutUserLoginResponse> Handle(PutUserLoginCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.Include(x => x.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_authorizationService.CheckUserPermissionRule(userAccount);

		var isNewLoginUnique = await _context.UserAccounts
			.AllAsync(x => x.Login != request.NewLogin, cancellationToken);

		if (!isNewLoginUnique)
			throw new ValidationProblem("Пользователь с таким логином уже существует, " +
				"новый логин должен быть уникальным");

		userAccount.Login = request.NewLogin;

		await _userAccountCache.SetLoginAsync(
			userAccount.Id,
			request.NewLogin,
			cancellationToken);

		userAccount.RevokeAllRefreshTokens();
		string refreshToken = _tokenService.CreateRefreshToken();
		userAccount.AddRefreshToken(new RefreshToken(refreshToken, userAccount));

		await _context.SaveChangesAsync(cancellationToken);

		return new PutUserLoginResponse
		{
			AccessToken = _tokenService.CreateAccessToken(userAccount),
			RefreshToken = refreshToken,
		};
	}
}
