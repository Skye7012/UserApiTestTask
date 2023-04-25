using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserApiTestTask.Infrastructure.Configs;

/// <summary>
/// Конфигурация для JWT
/// </summary>
public class JwtConfig
{
	/// <summary>
	/// Наименование секции в appSettings
	/// </summary>
	public const string ConfigSectionName = "JwtConfig";

	/// <summary>
	/// Секретный ключ
	/// </summary>
	public string Key { get; set; } = default!;

	/// <summary>
	/// Получатель токена
	/// </summary>
	public string Audience { get; set; } = default!;

	/// <summary>
	/// Производитель токена
	/// </summary>
	public string Issuer { get; set; } = default!;

	/// <summary>
	/// Длительность жизни Access токена в секундах
	/// </summary>
	public int AccessTokenLifeTime { get; set; } = 2 * 60;

	/// <summary>
	/// Длительность жизни Refresh токена в секундах
	/// </summary>
	public int RefreshTokenLifeTime { get; set; } = 60 * 60 * 24 * 7;

	/// <summary>
	/// Сформировать параметры валидации токена
	/// </summary>
	/// <returns>Параметры валидации токена</returns>
	public TokenValidationParameters BuildTokenValidationParameters()
		=> new()
		{
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(Key)),
			ValidIssuer = Issuer,
			ValidAudience = Audience,
			ValidateIssuerSigningKey = true,
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero,
		};
}
