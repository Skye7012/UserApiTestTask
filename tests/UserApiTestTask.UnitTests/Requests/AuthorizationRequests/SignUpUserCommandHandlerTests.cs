using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Application.Authorization.Commands.SignUp;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Exceptions;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.AuthorizationRequests;

/// <summary>
/// Тест для <see cref="SignUpCommandHandler"/>
/// </summary>
public class SignUpCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен создать пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldCreateUser_WhenCommandValid()
	{
		var command = new SignUpCommand
		{
			Login = "testlogin",
			Password = "testpassword",
			Name = "testname",
			Gender = Gender.Unknown,
			BirthDay = DateTimeProvider.UtcNow,
			IsAdmin = false,
		};

		using var context = await CreateInMemoryContextAsync();

		var handler = new SignUpCommandHandler(
			context,
			PasswordService);

		var response = await handler.Handle(command, default);

		var createdUser = context.Users
			.Include(x => x.UserAccount)
			.FirstOrDefault(x => x.Id == response.UserId);

		createdUser.Should().NotBeNull();
		createdUser!.Name.Should().Be(command.Name);
		createdUser!.BirthDay.Should().Be(command.BirthDay);
		createdUser!.Gender.Should().Be(command.Gender);
		createdUser!.IsAdmin.Should().Be(command.IsAdmin);

		createdUser.UserAccount.Should().NotBeNull();
		createdUser.UserAccount!.Login.Should().Be(command.Login);

		PasswordService.VerifyPasswordHash(
				command.Password,
				createdUser!.UserAccount.PasswordHash,
				createdUser!.UserAccount.PasswordSalt)
			.Should().BeTrue();

		PasswordService.Received(1)
			.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>());
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указан не уникальный логин
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenLoginIsNotUnique()
	{
		var command = new SignUpCommand
		{
			Login = AdminUserAccount.Login,
			Password = AdminUserAccount.Password,
		};

		using var context = await CreateInMemoryContextAsync();

		var handler = new SignUpCommandHandler(
			context,
			PasswordService);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Пользователь с таким логином уже существует");
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указан невалидный пароль
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenPasswordIsNotValid()
	{
		var command = new SignUpCommand
		{
			Login = "newUser",
			Password = "Невалидный пароль",
		};

		using var context = await CreateInMemoryContextAsync();

		var handler = new SignUpCommandHandler(
			context,
			PasswordService);

		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<ValidationProblem>()
			.WithMessage("Для пароля запрещены все символы кроме латинских букв и цифр");
	}
}
