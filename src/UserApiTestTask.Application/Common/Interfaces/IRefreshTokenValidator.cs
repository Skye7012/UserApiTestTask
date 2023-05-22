using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Валидатор Refresh токенов
/// </summary>
public interface IRefreshTokenValidator
{
	/// <summary>
	/// Провалидировать Refresh токен, и получить аккаунт пользователя, 
	/// которому принадлежит этот токен
	/// </summary>
	/// <param name="context">Контекст БД</param>
	/// <param name="refreshToken">Refresh токен</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Аккаунт пользователя, которому принадлежит этот токен</returns>
	public Task<UserAccount> ValidateAndReceiveUserAsync(
		IApplicationDbContext context,
		string refreshToken,
		CancellationToken cancellationToken = default);
}
