using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Authorization.Commands.PutUserPassword;

/// <summary>
/// Обработчик для <see cref="PutUserPasswordCommand"/>
/// </summary>
public class PutUserPasswordCommandHandler : IRequestHandler<PutUserPasswordCommand, PutUserPasswordResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly IAuthorizationService _authorizationService;
	private readonly IPasswordService _passwordService;
	private readonly ITokenService _tokenService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	/// <param name="passwordService">Сервис паролей</param>
	/// <param name="tokenService">Сервис JWT токенов</param>
	public PutUserPasswordCommandHandler(
		IApplicationDbContext context,
		IAuthorizationService authorizationService,
		IPasswordService passwordService,
		ITokenService tokenService)
	{
		_context = context;
		_authorizationService = authorizationService;
		_passwordService = passwordService;
		_tokenService = tokenService;
	}

	/// <inheritdoc/>
	public async Task<PutUserPasswordResponse> Handle(PutUserPasswordCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.Include(x => x.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		_authorizationService.CheckIsUserAdminOrUserItself(userAccount);

		if (!_passwordService.VerifyPasswordHash(request.OldPassword, userAccount.PasswordHash, userAccount.PasswordSalt))
			throw new ValidationProblem("Введен неверный текущий пароль пользователя");

		if (!Regex.IsMatch(request.NewPassword, @"^[a-zA-Z0-9]+$"))
			throw new ValidationProblem("Для пароля запрещены все символы кроме латинских букв и цифр");

		_passwordService.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

		userAccount.PasswordHash = passwordHash;
		userAccount.PasswordSalt = passwordSalt;

		userAccount.RevokeAllRefreshTokens();
		string refreshToken = _tokenService.CreateRefreshToken();
		userAccount.AddRefreshToken(new RefreshToken(refreshToken, userAccount));

		await _context.SaveChangesAsync(cancellationToken);

		return new PutUserPasswordResponse
		{
			AccessToken = _tokenService.CreateAccessToken(userAccount),
			RefreshToken = refreshToken,
		};
	}
}
