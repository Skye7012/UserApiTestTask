using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using UserApiTestTask.Infrastructure.Configs;
using UserApiTestTask.Infrastructure.Services;
using Xunit;

namespace UserApiTestTask.UnitTests.Services;

/// <summary>
/// Тесты для <see cref="RefreshTokenValidator"/>
/// </summary>
public class RefreshTokenValidatorTests : UnitTestBase
{
	private readonly JwtConfig _jwtConfig;
	private readonly RefreshTokenValidator _sut;

	/// <inheritdoc/>
	public RefreshTokenValidatorTests()
	{
		_jwtConfig = new JwtConfig()
		{
			Key = "JwtConfig key for unit tests",
			Audience = "Audience",
			Issuer = "Issuer",
		};

		_sut = new RefreshTokenValidator(_jwtConfig.BuildTokenValidationParameters());
	}

	/// <summary>
	/// Должен создать вернуть пользователя, когда токен валидный
	/// </summary>
	[Fact]
	public async Task ValidateAndReceiveUserAsync_ShoudReceiveUser_WhenTokenIsValid()
	{
		var tokenService = new TokenService(Options.Create(_jwtConfig));
		var refreshToken = tokenService.CreateRefreshToken();

		var context = await CreateInMemoryContextAsync(x => x.RefreshTokens.Add(new RefreshToken(refreshToken, AdminUserAccount)));

		var adminUser = await _sut.ValidateAndReceiveUserAsync(context, refreshToken, default);

		adminUser.Should().NotBeNull();
		adminUser.Id.Should().Be(AdminUserAccount.Id);
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда переданного Refresh токена не существует в БД
	/// </summary>
	[Fact]
	public async Task ValidateAndReceiveUserAsync_ShoudThrow_WhenRefreshTokenDoesntExist()
	{
		var tokenService = new TokenService(Options.Create(_jwtConfig));
		var refreshToken = tokenService.CreateRefreshToken();

		var context = await CreateInMemoryContextAsync();

		var act = async () => await _sut.ValidateAndReceiveUserAsync(context, refreshToken, default);

		await act.Should()
			.ThrowAsync<EntityNotFoundProblem<RefreshToken>>();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда Refresh токен не активный
	/// </summary>
	[Fact]
	public async Task ValidateAndReceiveUserAsync_ShoudThrow_WhenRefreshTokenRevoked()
	{
		var tokenService = new TokenService(Options.Create(_jwtConfig));
		var refreshToken = tokenService.CreateRefreshToken();

		var context = await CreateInMemoryContextAsync(x =>
			x.RefreshTokens.Add(new RefreshToken(refreshToken, AdminUserAccount)
			{
				RevokedOn = DateTime.UtcNow,
			}));

		var act = async () => await _sut.ValidateAndReceiveUserAsync(context, refreshToken, default);

		await act.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Refresh токен не активный");
	}

	/// <summary>
	/// Должен выкинуть ошибку и удалить невалидный токен, когда Refresh токен просрочен
	/// </summary>
	[Fact]
	public async Task ValidateAndReceiveUserAsync_ShoudRemoveInvalidToken_WhenRefreshTokenExpired()
	{
		var tokenService = new TokenService(Options.Create(_jwtConfig with { RefreshTokenLifeTime = 0 }));
		var refreshToken = tokenService.CreateRefreshToken();

		var context = await CreateInMemoryContextAsync(x => x.RefreshTokens.Add(new RefreshToken(refreshToken, AdminUserAccount)));

		var act = async () => await _sut.ValidateAndReceiveUserAsync(context, refreshToken, default);

		await act.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Refresh токен не валидный");

		context.Instance.ChangeTracker.Clear();

		var revokedRefreshToken = context.RefreshTokens
			.First(x => x.Token == refreshToken);

		revokedRefreshToken.RevokedOn.Should().NotBeNull();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда Refresh токен не валидный
	/// </summary>
	[Fact]
	public async Task ValidateAndReceiveUserAsync_ShoudThrow_WhenRefreshTokenNotValid()
	{
		var tokenService = new TokenService(Options.Create(_jwtConfig with { Issuer = "Invalid" }));
		var refreshToken = tokenService.CreateRefreshToken();

		var context = await CreateInMemoryContextAsync();

		var act = async () => await _sut.ValidateAndReceiveUserAsync(context, refreshToken, default);

		await act.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Refresh токен не валидный");
	}
}
