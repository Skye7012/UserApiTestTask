using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using UserApiTestTask.UnitTests.Mocks;
using Xunit;

namespace UserApiTestTask.UnitTests.Entities;

/// <summary>
/// Тесты для <see cref="UserAccount"/>
/// </summary>
public class UserAccountTests : UnitTestBase
{
	/// <summary>
	/// Должен выкинуть ошибку, когда указан пустой логин
	/// </summary>
	[Fact]
	public void Login_ShouldThrow_WhenEmptyLogin()
	{
		var context = CreateInMemoryContextAsync();

		var act = () => new UserAccount(
				" ",
				new byte[] { 1, 2 },
				new byte[] { 1, 2 });

		act.Should()
			.Throw<ValidationProblem>()
			.WithMessage($"Поле {nameof(UserAccount.Login)} не может быть пустым");
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указан невалидный логин
	/// </summary>
	[Fact]
	public void Login_ShouldThrow_WhenInvalidLogin()
	{
		var context = CreateInMemoryContextAsync();

		var act = () => new UserAccount(
				"Невалидный логин",
				new byte[] { 1, 2 },
				new byte[] { 1, 2 });

		act.Should()
			.Throw<ValidationProblem>()
			.WithMessage($"Для логина запрещены все символы кроме латинских букв и цифр");
	}

	/// <summary>
	/// Должен деактивировать все токены
	/// </summary>
	[Fact]
	public async Task RevokeAllRefreshTokens_ShouldRevokeAllTokens()
	{
		var createRefreshToken = (int n) => new RefreshToken(TokenServiceSubstitute.RefreshTokenName + n, AdminUserAccount);

		var context = await CreateInMemoryContextAsync();

		var adminUserAccount = context.UserAccounts
			.Include(x => x.RefreshTokens)
			.First(x => x.Id == AdminUserAccount.Id);

		foreach (var n in Enumerable.Range(1, 5))
			adminUserAccount.AddRefreshToken(createRefreshToken(n));

		adminUserAccount.RevokeAllRefreshTokens();

		adminUserAccount.RefreshTokens.Should().BeEmpty();
	}

	/// <summary>
	/// При добавлении шестого активного токена должен деактивировать самый старый активный токен
	/// </summary>
	[Fact]
	public async Task AddRefreshToken_ShouldRevokeExtraTokens_WhenAddingSixthActiveToken()
	{
		int i = 1;
		DateTimeProvider.UtcNow
			.Returns(x => DateTime.UtcNow.AddHours(i++));

		var createRefreshToken = (int n) => new RefreshToken(TokenServiceSubstitute.RefreshTokenName + n, AdminUserAccount)
		{
			CreatedOn = DateTimeProvider.UtcNow,
		};

		var context = await CreateInMemoryContextAsync();

		var adminUserAccount = context.UserAccounts
			.Include(x => x.RefreshTokens)
			.First(x => x.Id == AdminUserAccount.Id);

		foreach (var n in Enumerable.Range(1, 4))
			adminUserAccount.AddRefreshToken(createRefreshToken(n));

		adminUserAccount.AddRefreshToken(new RefreshToken("RevokedToken", AdminUserAccount)
		{
			RevokedOn = DateTime.UtcNow
		});

		adminUserAccount.AddRefreshToken(createRefreshToken(5));

		adminUserAccount.AddRefreshToken(createRefreshToken(6));

		adminUserAccount.RefreshTokens.Should()
			.NotContain(x => x.Token == TokenServiceSubstitute.RefreshTokenName + 5);

		adminUserAccount.RefreshTokens.Should()
			.ContainSingle(x => x.Token == "RevokedToken");

		adminUserAccount.RefreshTokens.Should()
			.ContainSingle(x => x.Token == TokenServiceSubstitute.RefreshTokenName + 6);
	}
}
