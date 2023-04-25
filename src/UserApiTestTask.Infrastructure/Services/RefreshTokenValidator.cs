using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Infrastructure.Services;

/// <summary>
/// Валидатор Refresh токенов
/// </summary>
public class RefreshTokenValidator : IRefreshTokenValidator
{
	private readonly TokenValidationParameters _tokenValidationParameters;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="tokenValidationParameters">Параметры валидации токена</param>
	public RefreshTokenValidator(TokenValidationParameters tokenValidationParameters)
		=> _tokenValidationParameters = tokenValidationParameters;

	/// <inheritdoc/>
	public async Task<UserAccount> ValidateAndReceiveUserAsync(
		IApplicationDbContext context,
		string refreshToken,
		CancellationToken cancellationToken = default)
	{
		var receiveRefreshTokenAndRemoveFromDBAsync = async () =>
		{
			var refreshTokenEntity = await context.RefreshTokens
				.Include(x => x.UserAccount)
					.ThenInclude(x => x!.User) // Нужен для клеймов при дальнейшем создании access токена
				.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken)
				?? throw new EntityNotFoundProblem<RefreshToken>(
					nameof(RefreshToken.Token),
					refreshToken);

			context.RefreshTokens.Remove(refreshTokenEntity);

			return refreshTokenEntity;
		};

		var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
		try
		{
			jwtSecurityTokenHandler.ValidateToken(refreshToken, _tokenValidationParameters, out var _);
			var refreshTokenEntity = await receiveRefreshTokenAndRemoveFromDBAsync();

			return refreshTokenEntity.RevokedOn != null
				? throw new ValidationProblem("Refresh токен не активный")
				: refreshTokenEntity.UserAccount!;
		}
		catch (Exception exception)
		{
			if (exception is SecurityTokenExpiredException expiredException)
			{
				await receiveRefreshTokenAndRemoveFromDBAsync();
				await context.SaveChangesAsync(cancellationToken);
			}

			if (exception is ApplicationProblem applicationProblem)
				throw applicationProblem;

			throw new ValidationProblem("Refresh токен не валидный");
		}
	}
}
