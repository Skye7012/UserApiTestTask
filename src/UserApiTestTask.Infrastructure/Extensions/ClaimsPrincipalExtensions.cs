using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Static;

namespace UserApiTestTask.Infrastructure.Extensions;

/// <summary>
/// Расширения для <see cref="ClaimsPrincipal"/>
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Аутентифицировался ли пользователь
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <returns>Аутентифицировался ли пользователь</returns>
	public static bool IsAuthenticated(this ClaimsPrincipal? source)
		=> source?.FindFirstValue(CustomClaims.UserIdСlaimName) != null
			&& source?.FindFirstValue(CustomClaims.UserAccountIdClaimName) != null
			&& source?.FindFirstValue(CustomClaims.IsAdminClaimName) != null;

	/// <summary>
	/// Получить идентификатор пользователя
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <returns>Идентификатор пользователя</returns>
	public static Guid GetUserId(this ClaimsPrincipal? source)
	{
		var isParsed = Guid.TryParse(
				source?.FindFirstValue(CustomClaims.UserIdСlaimName),
				out var res);

		return !isParsed
			? throw new UnauthorizedProblem($"Невалидный клейм '{CustomClaims.UserIdСlaimName}' " +
				$"в токене")
			: res;
	}

	/// <summary>
	/// Получить идентификатор аккаунта пользователя
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <returns>Идентификатор аккаунта пользователя</returns>
	public static Guid GetUserAccountId(this ClaimsPrincipal? source)
	{
		var isParsed = Guid.TryParse(
				source?.FindFirstValue(CustomClaims.UserAccountIdClaimName),
				out var res);

		return !isParsed
			? throw new UnauthorizedProblem($"Невалидный клейм '{CustomClaims.UserAccountIdClaimName}' " +
				$"в токене")
			: res;
	}

	/// <summary>
	/// Проверить является ли пользователь администратором
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <returns>Является ли пользователь администратором</returns>
	public static bool GetIsAdmin(this ClaimsPrincipal? source)
	{
		var isParsed = bool.TryParse(
				source?.FindFirstValue(CustomClaims.IsAdminClaimName),
				out var res);

		return !isParsed
			? throw new UnauthorizedProblem($"Невалидный клейм '{CustomClaims.IsAdminClaimName}' " +
				$"в токене")
			: res;
	}

	/// <summary>
	/// Проверить является ли пользователь администратором
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <returns>Является ли пользователь администратором</returns>
	public static DateTime GetExpired(this ClaimsPrincipal? source)
	{
		var isParsed = long.TryParse(
				source?.FindFirstValue(JwtRegisteredClaimNames.Exp),
				out var unixTimeSeconds);

		return isParsed
			? throw new UnauthorizedProblem($"Невалидный клейм '{JwtRegisteredClaimNames.Exp}' " +
				$"в токене")
			: DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
	}
}
