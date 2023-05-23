using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Infrastructure.Configs;
using UserApiTestTask.Infrastructure.Services;
using Xunit;

namespace UserApiTestTask.UnitTests.Services;

/// <summary>
/// Тесты для <see cref="TokenService"/>
/// </summary>
public class TokenServiceTests : UnitTestBase
{
	private readonly TokenService _sut;
	private readonly JwtConfig _jwtConfig;

	/// <inheritdoc/>
	public TokenServiceTests()
	{
		_jwtConfig = new JwtConfig()
		{
			Key = "JwtConfig key for unit tests",
			Audience = "Audience",
			Issuer = "Issuer",
		};

		var jwtConfigOptions = Options.Create(_jwtConfig);

		_sut = new TokenService(jwtConfigOptions);
	}

	/// <summary>
	/// Должен создать правильный JWT токен
	/// </summary>
	[Fact]
	public void CreateToken_ShoudCreateValidJwtToken()
	{
		var tokenString = _sut.CreateToken(default);

		var token = new JwtSecurityToken(tokenString);

		token.Issuer.Should().Be("Issuer");
		token.Audiences.Single().Should().Be("Audience");
		token.SignatureAlgorithm.Should().Be(SecurityAlgorithms.HmacSha512Signature);
	}

	/// <summary>
	/// Должен создать правильный Access токен с правильными клеймами
	/// </summary>
	[Fact]
	public async Task CreateAccessToken_ShoudCreateValidAccessToken()
	{
		await CreateInMemoryContextAsync();

		var tokenString = _sut.CreateAccessToken(AdminUserAccount);

		var token = new JwtSecurityToken(tokenString);

		var userIdClaimValue = token.Claims
			.First(x => x.Type == CustomClaims.UserIdСlaimName)
			.Value;

		userIdClaimValue.Should().Be(AdminUserAccount.User!.Id.ToString());

		var userAccountIdClaimValue = token.Claims
			.First(x => x.Type == CustomClaims.UserAccountIdClaimName)
			.Value;

		userAccountIdClaimValue.Should().Be(AdminUserAccount.Id.ToString());

		var isAdminClaimValue = bool.Parse(token.Claims
			.First(x => x.Type == CustomClaims.IsAdminClaimName)
			.Value);

		token.ValidTo.Should().BeCloseTo(
			DateTime.UtcNow.AddSeconds(_jwtConfig.AccessTokenLifeTime),
			TimeSpan.FromSeconds(1));
	}

	/// <summary>
	/// Должен создать правильный Refresh токен с правильным lifetime
	/// </summary>
	[Fact]
	public void CreateRefreshToken_ShoudCreateValidRefreshToken()
	{
		var tokenString = _sut.CreateRefreshToken();

		var token = new JwtSecurityToken(tokenString);

		token.ValidTo.Should().BeCloseTo(
			DateTime.UtcNow.AddSeconds(_jwtConfig.RefreshTokenLifeTime),
			TimeSpan.FromSeconds(1));
	}
}
