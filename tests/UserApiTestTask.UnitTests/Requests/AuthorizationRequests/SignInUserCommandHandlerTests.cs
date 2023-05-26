using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using UserApiTestTask.Application.Authorization.Commands.SignIn;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.AuthorizationRequests;

/// <summary>
/// Тест для <see cref="SignInCommandHandler"/>
/// </summary>
public class SignInCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен создать пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldCreateUser_WhenCommandValid()
	{
		var command = new SignInCommand
		{
			Login = AdminUserAccount.Login,
			Password = AdminUserAccount.Password,
		};

		using var context = await CreateInMemoryContextAsync();

		var accessToken = TokenService.CreateAccessToken(AdminUserAccount);
		var refreshToken = TokenService.CreateRefreshToken();
		TokenService.ClearReceivedCalls();

		var handler = new SignInCommandHandler(
			context,
			TokenService,
			PasswordService);

		var response = await handler.Handle(command, default);

		response.AccessToken.Should().NotBeNullOrWhiteSpace();
		response.AccessToken.Should().Be(accessToken);

		response.RefreshToken.Should().NotBeNullOrWhiteSpace();
		response.RefreshToken.Should().Be(refreshToken);

		PasswordService.Received(1)
			.VerifyPasswordHash(command.Password, AdminUserAccount.PasswordHash, AdminUserAccount.PasswordSalt);

		TokenService.Received(1)
			.CreateRefreshToken();

		TokenService.Received(1)
			.CreateAccessToken(Arg.Is<UserAccount>(x => x.Id == AdminUserAccount.Id));
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		var command = new SignInCommand
		{
			Login = "NotExistingLogin",
			Password = AdminUserAccount.Password,
		};

		using var context = await CreateInMemoryContextAsync();

		var handler = new SignInCommandHandler(context, TokenService, PasswordService);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь деактивирован
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserBlocked()
	{
		var command = new SignInCommand
		{
			Login = AdminUserAccount.Login,
			Password = AdminUserAccount.Password,
		};

		using var context = await CreateInMemoryContextAsync();

		context.UserAccounts.Remove(AdminUserAccount);
		await context.SaveChangesAsync();

		var handler = new SignInCommandHandler(context, TokenService, PasswordService);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Данные учетные данные были деактивированы");
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указан неправильный пароль
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenPasswordIsWrong()
	{
		var command = new SignInCommand
		{
			Login = AdminUserAccount.Login,
			Password = AdminUserAccount.Password + "NotCorrectPassword",
		};

		using var context = await CreateInMemoryContextAsync();

		var handler = new SignInCommandHandler(context, TokenService, PasswordService);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Неправильный пароль");
	}
}
