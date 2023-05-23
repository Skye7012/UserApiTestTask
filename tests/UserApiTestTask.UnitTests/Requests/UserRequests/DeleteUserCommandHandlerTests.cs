using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Users.Commands.DeleteUser;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="DeleteUserCommandHandler"/>
/// </summary>
public class DeleteUserCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен удалить пользователя мягко, когда указано использование мягкого удаления
	/// </summary>
	[Fact]
	public async Task Handle_ShouldSoftDeleleteUser_WhenWithSoftDelete()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new DeleteUserCommand
		{
			Login = AdminUserAccount.Login,
			WithSoftDelete = true,
		};

		var handler = new DeleteUserCommandHandler(context);
		await handler.Handle(command, default);

		var adminUserAccount = context.UserAccounts
			.FirstOrDefault(x => x.Id == AdminUserAccount.Id);

		adminUserAccount.Should().NotBeNull();
		adminUserAccount!.RevokedOn.Should().NotBeNull();
	}

	/// <summary>
	/// Должен удалить пользователя жестко, когда не указано использование мягкого удаления
	/// </summary>
	[Fact]
	public async Task Handle_ShouldHardDeleleteUser_WhenNotWithSoftDelete()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new DeleteUserCommand
		{
			Login = AdminUserAccount.Login,
			WithSoftDelete = false,
		};

		var handler = new DeleteUserCommandHandler(context);
		await handler.Handle(command, default);

		var adminUserAccount = context.UserAccounts
			.FirstOrDefault(x => x.Id == AdminUserAccount.Id);

		adminUserAccount.Should().BeNull();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new DeleteUserCommand
		{
			Login = AdminUserAccount.Login + "NotExisting",
			WithSoftDelete = true,
		};

		var handler = new DeleteUserCommandHandler(context);
		var handle = async () => await handler.Handle(command, default); ;

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}
}
