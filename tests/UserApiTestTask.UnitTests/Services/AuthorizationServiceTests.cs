using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Services;
using Xunit;

namespace UserApiTestTask.UnitTests.Services;

/// <summary>
/// Тесты для <see cref="AuthorizationService"/>
/// </summary>
public class AuthorizationServiceTests : UnitTestBase
{
	/// <summary>
	/// Сформировать тестируемый сервис
	/// </summary>
	/// <param name="claims">Клеймы</param>
	/// <param name="userAccount">Аккаунт Пользователь</param>
	/// <returns>Тестируемый сервис</returns>
	private AuthorizationService BuildSut(
		List<Claim>? claims = null,
		UserAccount? userAccount = null)
	{
		userAccount ??= AdminUserAccount;
		claims ??= new List<Claim>
		{
			new Claim(CustomClaims.UserIdСlaimName, userAccount.User!.Id.ToString()),
			new Claim(CustomClaims.UserAccountIdClaimName, userAccount.Id.ToString()),
			new Claim(CustomClaims.IsAdminClaimName, userAccount.User!.IsAdmin.ToString()),
		};

		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		var context = new DefaultHttpContext();
		context.User.AddIdentity(new ClaimsIdentity(claims));
		httpContextAccessor.HttpContext
			.Returns(context);

		return new AuthorizationService(httpContextAccessor);
	}

	/// <summary>
	/// Сформировать тестируемый сервис
	/// </summary>
	/// <param name="userAccount">Аккаунт Пользователь</param>
	/// <returns>Тестируемый сервис</returns>
	private AuthorizationService BuildSut(UserAccount userAccount)
		=> BuildSut(null, userAccount);

	/// <summary>
	/// Все методы должны правильно вызываться с правильными клеймами пользователя-администратора <br/>
	/// ultimate happy path
	/// </summary>
	[Fact]
	public void AllMethods_ShouldWork_WithValidClaimsOfAdminUser()
	{
		var sut = BuildSut();
		sut.IsAuthenticated().Should().BeTrue();
		sut.IsAdmin().Should().BeTrue();
		sut.GetUserId().Should().Be(AdminUserAccount.User!.Id);
		sut.GetUserAccountId().Should().Be(AdminUserAccount.Id);

		sut.CheckUserPermissionRule(AdminUserAccount);
	}

	/// <summary>
	/// Должен выкинуть ошибку при невалидных клеймах
	/// </summary>
	[Fact]
	public void IsAuthenticated_ShouldThrow_WhenInvalidClaims()
	{
		var sut = BuildSut(new List<Claim>());
		sut.IsAuthenticated().Should().BeFalse();
	}

	/// <summary>
	/// Должен выкинуть ошибку при невалидном клейме
	/// </summary>
	[Fact]
	public void IsAdmin_ShouldThrow_WhenInvalidClaim()
	{
		var sut = BuildSut(new List<Claim>()
		{
			new Claim(CustomClaims.IsAdminClaimName, "Invalid")
		});

		var act = () => sut.IsAdmin();

		act.Should()
			.Throw<UnauthorizedProblem>()
			.WithMessage($"Невалидный клейм '{CustomClaims.IsAdminClaimName}' " +
				$"в токене");
	}

	/// <summary>
	/// Должен выкинуть ошибку при невалидном клейме
	/// </summary>
	[Fact]
	public void GetUserId_ShouldThrow_WhenInvalidClaim()
	{
		var sut = BuildSut(new List<Claim>()
		{
			new Claim(CustomClaims.UserIdСlaimName, "Invalid")
		});

		var act = () => sut.GetUserId();

		act.Should()
			.Throw<UnauthorizedProblem>()
			.WithMessage($"Невалидный клейм '{CustomClaims.UserIdСlaimName}' " +
				$"в токене");
	}

	/// <summary>
	/// Должен выкинуть ошибку при невалидном клейме
	/// </summary>
	[Fact]
	public void GetUserAccountId_ShouldThrow_WhenInvalidClaim()
	{
		var sut = BuildSut(new List<Claim>()
		{
			new Claim(CustomClaims.UserAccountIdClaimName, "Invalid")
		});

		var act = () => sut.GetUserAccountId();

		act.Should()
			.Throw<UnauthorizedProblem>()
			.WithMessage($"Невалидный клейм '{CustomClaims.UserAccountIdClaimName}' " +
				$"в токене");
	}

	/// <summary>
	/// Должен пройти проверку при передаче правильного пользователя
	/// </summary>
	[Fact]
	public async Task CheckUserPermissionRule_ShouldPass_WhenValidUser()
	{
		await CreateInMemoryContextAsync();

		var userAccount = new UserAccount
		{
			Login = "new",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = false,
				Name = "newUser",
				Gender = Gender.Unknown,
			}
		};

		var sut = BuildSut(userAccount);

		sut.CheckUserPermissionRule(userAccount);
	}

	/// <summary>
	/// Должен выкинуть ошибку при передаче неправильного пользователя
	/// </summary>
	[Fact]
	public async Task CheckUserPermissionRule_ShouldThrow_WhenInvalidUser()
	{
		var userAccount = new UserAccount
		{
			Login = "new",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = false,
				Name = "newUser",
				Gender = Gender.Unknown,
			}
		};

		var context = await CreateInMemoryContextAsync();

		context.UserAccounts.Add(userAccount);
		await context.SaveChangesAsync();

		var sut = BuildSut(
			new List<Claim>()
			{
				new Claim(CustomClaims.UserIdСlaimName, AdminUserAccount.User!.Id.ToString()),
				new Claim(CustomClaims.UserAccountIdClaimName, AdminUserAccount.Id.ToString()),
				new Claim(CustomClaims.IsAdminClaimName, false.ToString())
			},
			userAccount);

		var act = () => sut.CheckUserPermissionRule(userAccount);

		act.Should()
			.Throw<ForbiddenProblem>()
			.WithMessage("Данное действие доступно администратору, " +
				"либо лично пользователю, если он активен");
	}

	/// <summary>
	/// Должен выкинуть ошибку при передаче неактивного пользователя
	/// </summary>
	[Fact]
	public void CheckUserPermissionRule_ShouldThrow_WhenBlockedUser()
	{
		var userAccount = new UserAccount
		{
			Login = "new",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			RevokedOn = DateTime.UtcNow,
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = false,
				Name = "newUser",
				Gender = Gender.Unknown,
			}
		};

		var context = CreateInMemoryContextAsync(async x => await x.UserAccounts.AddAsync(userAccount));

		var sut = BuildSut(userAccount);

		var act = () => sut.CheckUserPermissionRule(userAccount);

		act.Should()
			.Throw<ForbiddenProblem>()
			.WithMessage("Данное действие доступно администратору, " +
				"либо лично пользователю, если он активен");
	}
}
