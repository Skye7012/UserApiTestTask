using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Infrastructure.Services;

/// <inheritdoc/>
public class UserService : IUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _configuration;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="httpContextAccessor">HTTP контекст запроса</param>
	/// <param name="configuration">Конфигурация приложения</param>
	public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
	{
		_httpContextAccessor = httpContextAccessor;
		_configuration = configuration;
	}

	/// <inheritdoc/>
	public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
	{
		using var hmac = new HMACSHA512();

		passwordSalt = hmac.Key;
		passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
	}

	/// <inheritdoc/>
	public string CreateToken(User user)
	{
		List<Claim> claims = new()
		{
			new Claim(ClaimTypes.NameIdentifier, user.Login),
			new Claim(CustomClaims.IsAdminClaimName, user.IsAdmin.ToString()),
		};

		var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
			_configuration.GetSection("AppSettings:Token").Value!));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddDays(1),
			signingCredentials: creds);

		var jwt = new JwtSecurityTokenHandler().WriteToken(token);

		return jwt;
	}

	/// <inheritdoc/>
	public bool IsAuthenticated()
		=> !string.IsNullOrWhiteSpace(
			_httpContextAccessor.HttpContext?.User
				.FindFirstValue(ClaimTypes.NameIdentifier));

	/// <inheritdoc/>
	public string GetLogin()
	{
		var result = string.Empty;
		if (_httpContextAccessor.HttpContext != null)
		{
			result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
		}
		return result;
	}

	/// <inheritdoc/>
	public bool IsAdmin()
	{
		var result = false;
		if (_httpContextAccessor.HttpContext != null)
		{
			result = bool.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(CustomClaims.IsAdminClaimName));
		}
		return result;
	}

	/// <inheritdoc/>
	public void CheckUserPermissionRule(User user)
	{
		if (IsAdmin())
			return;

		if (user.Login != GetLogin() || user.RevokedOn != null)
			throw new ForbiddenProblem();
	}

	/// <inheritdoc/>
	public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
	{
		using var hmac = new HMACSHA512(passwordSalt);

		var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
		return computedHash.SequenceEqual(passwordHash);
	}
}
