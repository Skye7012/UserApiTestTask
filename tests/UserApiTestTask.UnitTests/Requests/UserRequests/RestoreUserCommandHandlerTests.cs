using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Users.Commands.RestoreUser;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
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

		using var context = await CreateInMemoryContextAsync(x => x.UserAccounts.Add(userAccount));

		context.UserAccounts.Remove(userAccount);
		await context.SaveChangesAsync();
		context.Instance.ChangeTracker.Clear();

		var command = new RestoreUserCommand(userAccount.Login);
		var handler = new RestoreUserCommandHandler(context, AuthorizationService);
		await handler.Handle(command, default);

		var adminUserAccount = context.UserAccounts
			.Include(x => x.User)
			.First(x => x.Id == AdminUserAccount.Id);

		adminUserAccount.RevokedOn.Should().BeNull();
		adminUserAccount.RevokedBy.Should().BeNull();
		adminUserAccount.User!.RevokedOn.Should().BeNull();
		adminUserAccount.User!.RevokedBy.Should().BeNull();

		AuthorizationService
			.Received(1)
			.CheckIsAdmin();
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new RestoreUserCommand("NotExistingLogin");
		var handler = new RestoreUserCommandHandler(context, AuthorizationService);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}
}
