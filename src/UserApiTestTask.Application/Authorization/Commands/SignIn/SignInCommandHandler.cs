using MediatR;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Authorization.Commands.SignIn;

/// <summary>
/// Обработчик для <see cref="SignInCommand"/>
/// </summary>
public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInResponse>
{
	private readonly IApplicationDbContext _context;
	private readonly ITokenService _tokenService;
	private readonly IPasswordService _passwordService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="tokenService">Сервис JWT токенов</param>
	/// <param name="passwordService">Сервис паролей</param>
	public SignInCommandHandler(
		IApplicationDbContext context,
		ITokenService tokenService,
		IPasswordService passwordService)
	{
		_context = context;
		_tokenService = tokenService;
		_passwordService = passwordService;
	}

	/// <inheritdoc/>
	public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
	{
		var userAccount = await _context.UserAccounts
			.Include(x => x.User)
			.Include(x => x.RefreshTokens)
			.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken)
			?? throw new UserNotFoundProblem(request.Login);

		if (userAccount.RevokedOn != null)
			throw new ValidationProblem("Данные учетные данные были деактивированы");

		if (!_passwordService.VerifyPasswordHash(request.Password, userAccount.PasswordHash, userAccount.PasswordSalt))
		{
			throw new ValidationProblem("Неправильный пароль");
		}

		string refreshToken = _tokenService.CreateRefreshToken();
		userAccount.AddRefreshToken(new RefreshToken(refreshToken, userAccount));

		await _context.SaveChangesAsync(cancellationToken);

		return new SignInResponse()
		{
			AccessToken = _tokenService.CreateAccessToken(userAccount),
			RefreshToken = refreshToken,
		};
	}
}
