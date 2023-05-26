using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Application.Authorization.Commands.PutUserPassword;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.AuthorizationRequests;

/// <summary>
/// Тест для <see cref="PutUserPasswordCommandHandler"/>
/// </summary>
public class PutUserPasswordCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен изменить пароль пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldUpdateUserPasswod_WhenCommandValid()
	{
		var accessToken = TokenService.CreateAccessToken(AdminUserAccount);
		var refreshToken = TokenService.CreateRefreshToken();

		using var context = await CreateInMemoryContextAsync(x =>
		{
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "1", AdminUserAccount));
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "2", AdminUserAccount));
		});

		TokenService.ClearReceivedCalls();

		var command = new PutUserPasswordCommand(AdminUserAccount.Login)
		{
			OldPassword = AdminUserAccount.Password,
			NewPassword = "NewAdminPassword",
		};

		var handler = new PutUserPasswordCommandHandler(
			context,
			AuthorizationService,
			PasswordService,
			TokenService);

		var response = await handler.Handle(command, default);

		context.Instance.ChangeTracker.Clear();
		var updatedUser = context.UserAccounts
			.Include(x => x.RefreshTokens)
			.FirstOrDefault(x => x.Id == AdminUserAccount.Id);

		updatedUser.Should().NotBeNull();

		PasswordService.Received(1)
			.VerifyPasswordHash(command.OldPassword, AdminUserAccount.PasswordHash, AdminUserAccount.PasswordSalt);

		PasswordService.VerifyPasswordHash(
				command.NewPassword,
				updatedUser!.PasswordHash,
				updatedUser!.PasswordSalt)
			.Should().BeTrue();

		updatedUser.RefreshTokens.Should().NotBeNullOrEmpty();
		updatedUser.RefreshTokens.Should().ContainSingle(x => x.RevokedOn == null);

		AuthorizationService.Received(1)
			.CheckUserPermissionRule(Arg.Is<UserAccount>(u => u.Id == AdminUserAccount.Id));

		PasswordService.Received(1)
			.CreatePasswordHash(
				command.NewPassword,
				out Arg.Is<byte[]>(x => x.SequenceEqual(updatedUser.PasswordHash)),
				out Arg.Is<byte[]>(x => x.SequenceEqual(updatedUser.PasswordSalt)));

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

		var command = new PutUserPasswordCommand("NotExistingLogin")
		{
			OldPassword = AdminUserAccount.Password,
			NewPassword = "NewPassword",
		};

		var handler = new PutUserPasswordCommandHandler(
			context,
			AuthorizationService,
			PasswordService,
			TokenService);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<EntityNotFoundProblem<User>>();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда введен не правильный текущий пароль
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenOldPasswordIsWrong()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new PutUserPasswordCommand(AdminUserAccount.Login)
		{
			OldPassword = AdminUserAccount.Password + "Wrong",
			NewPassword = "NewPassword",
		};

		var handler = new PutUserPasswordCommandHandler(
			context,
			AuthorizationService,
			PasswordService,
			TokenService);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>("Введен неверный текущий пароль пользователя");
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указан невалидный пароль
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenPasswordIsNotValid()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new PutUserPasswordCommand(AdminUserAccount.Login)
		{
			OldPassword = AdminUserAccount.Password,
			NewPassword = "Невалидный пароль",
		};

		var handler = new PutUserPasswordCommandHandler(
			context,
			AuthorizationService,
			PasswordService,
			TokenService);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Для пароля запрещены все символы кроме латинских букв и цифр");
	}
}
