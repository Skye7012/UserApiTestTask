using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using UserApiTestTask.Infrastructure.Configs;

namespace UserApiTestTask.Infrastructure.Services;

/// <summary>
/// Сервис JWT токенов
/// </summary>
public class TokenService : ITokenService
{
	private readonly JwtConfig _jwtConfig;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="jwtConfig">Конфигурация для JWT</param>
	public TokenService(IOptions<JwtConfig> jwtConfig)
		=> _jwtConfig = jwtConfig.Value;

	/// <inheritdoc/>
	public string CreateAccessToken(UserAccount userAccount)
	{
		if (userAccount.User == null)
			throw new NotIncludedProblem(nameof(UserAccount.User));

		List<Claim> claims = new()
		{
			new Claim(CustomClaims.UserIdСlaimName, userAccount.User!.Id.ToString()),
			new Claim(CustomClaims.UserAccountIdClaimName, userAccount.Id.ToString()),
			new Claim(CustomClaims.IsAdminClaimName, userAccount.User!.IsAdmin.ToString()),
		};

		return CreateToken(
			DateTime.UtcNow.AddSeconds(_jwtConfig.AccessTokenLifeTime),
			claims);
	}

	/// <inheritdoc/>
	public string CreateRefreshToken()
	{
		List<Claim> claims = new()
		{
			new Claim(CustomClaims.IdСlaimName, Guid.NewGuid().ToString()),
		};

		return CreateToken(
			DateTime.UtcNow.AddSeconds(_jwtConfig.RefreshTokenLifeTime),
			claims);
	}

	/// <inheritdoc/>
	public string CreateToken(DateTime expires, IEnumerable<Claim>? claims = null)
	{
		var key = new SymmetricSecurityKey(
			System.Text.Encoding.UTF8.GetBytes(_jwtConfig.Key));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

		var token = new JwtSecurityToken(
			claims: claims,
			expires: expires,
			issuer: _jwtConfig.Issuer,
			audience: _jwtConfig.Audience,
			signingCredentials: creds);

		var jwt = new JwtSecurityTokenHandler().WriteToken(token);

		return jwt;
	}
}
