using System.Security.Claims;
using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Сервис JWT токенов
/// </summary>
public interface ITokenService
{
	/// <summary>
	/// Создать Access токен
	/// </summary>
	/// <param name="userAccount">Аккаунт пользователя, для которого создается токен</param>
	/// <returns>Токен</returns>
	string CreateAccessToken(UserAccount userAccount);

	/// <summary>
	/// Создать Reresh токен
	/// </summary>
	/// <returns>Токен</returns>
	string CreateRefreshToken();

	/// <summary>
	/// Создать JWT токен
	/// </summary>
	/// <param name="expires">Время окончания действия токена</param>
	/// <param name="claims">Клеймы</param>
	/// <returns>Токен</returns>
	public string CreateToken(DateTime expires, IEnumerable<Claim>? claims = null);
}
