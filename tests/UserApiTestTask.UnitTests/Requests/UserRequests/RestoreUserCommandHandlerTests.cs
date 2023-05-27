using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Users.Commands.RestoreUser;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="RestoreUserCommandHandler"/>
/// </summary>
public class RestoreUserCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен восстановить пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldRestoreUser_WhenCommandValid()
	{
		using var context = await CreateInMemoryContextAsync();

		context.UserAccounts.Remove(AdminUserAccount);
		await context.SaveChangesAsync();
		context.Instance.ChangeTracker.Clear();

		var command = new RestoreUserCommand(AdminUserAccount.Login);
		var handler = new RestoreUserCommandHandler(context);
		await handler.Handle(command, default);

		var adminUserAccount = context.UserAccounts
			.Include(x => x.User)
			.First(x => x.Id == AdminUserAccount.Id);

		adminUserAccount.RevokedOn.Should().BeNull();
		adminUserAccount.RevokedBy.Should().BeNull();
		adminUserAccount.User!.RevokedOn.Should().BeNull();
		adminUserAccount.User!.RevokedBy.Should().BeNull();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		context.UserAccounts.Remove(AdminUserAccount);
		await context.SaveChangesAsync();
		context.Instance.ChangeTracker.Clear();

		var command = new RestoreUserCommand("NotExistingLogin");
		var handler = new RestoreUserCommandHandler(context);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}
}
