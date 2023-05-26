using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Application.Authorization.Commands.PutUserLogin;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.AuthorizationRequests;

/// <summary>
/// Тест для <see cref="PutUserLoginCommandHandler"/>
/// </summary>
public class PutUserLoginCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен изменить логин пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldUpdateUserLogin_WhenCommandValid()
	{
		var accessToken = TokenService.CreateAccessToken(AdminUserAccount);
		var refreshToken = TokenService.CreateRefreshToken();

		using var context = await CreateInMemoryContextAsync(x =>
		{
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "1", AdminUserAccount));
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "2", AdminUserAccount));
		});

		TokenService.ClearReceivedCalls();

		var command = new PutUserLoginCommand(AdminUserAccount.Login)
		{
			NewLogin = "NewAdmin",
		};

		var handler = new PutUserLoginCommandHandler(
			context,
			AuthorizationService,
			TokenService,
			UserAccountCacheRepository);

		var response = await handler.Handle(command, default);

		context.Instance.ChangeTracker.Clear();
		var updatedUserAccount = context.UserAccounts
			.Include(x => x.RefreshTokens)
			.FirstOrDefault(x => x.Id == AdminUserAccount.Id);

		updatedUserAccount.Should().NotBeNull();
		updatedUserAccount!.Login.Should().Be(command.NewLogin);

		updatedUserAccount.RefreshTokens.Should().NotBeNullOrEmpty();
		updatedUserAccount.RefreshTokens.Should().ContainSingle(x => x.RevokedOn == null);

		AuthorizationService.Received(1)
			.CheckUserPermissionRule(Arg.Is<UserAccount>(u => u.Id == AdminUserAccount.Id));

		TokenService.Received(1)
			.CreateRefreshToken();

		TokenService.Received(1)
			.CreateAccessToken(Arg.Is<UserAccount>(u => u.Id == AdminUserAccount.Id));
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new PutUserLoginCommand("NotExistingLogin")
		{
			NewLogin = "NewAdmin",
		};

		var handler = new PutUserLoginCommandHandler(
			context,
			AuthorizationService,
			TokenService,
			UserAccountCacheRepository);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь с таким логином уже существует
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenNewLoginIsNotUnique()
	{
		using var context = await CreateInMemoryContextAsync(x =>
			x.UserAccounts.Add(new UserAccount
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
			}));

		var command = new PutUserLoginCommand("new")
		{
			NewLogin = AdminUserAccount.Login,
		};

		var handler = new PutUserLoginCommandHandler(
			context,
			AuthorizationService,
			TokenService,
			UserAccountCacheRepository);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>("Пользователь с таким логином уже существует, " +
				"новый логин должен быть уникальным");
	}
}
