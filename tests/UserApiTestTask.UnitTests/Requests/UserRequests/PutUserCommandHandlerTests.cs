using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Users.Commands.PutUser;
using UserApiTestTask.Contracts.Common.Enums;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="PutUserCommandHandler"/>
/// </summary>
public class PutUserCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен обновить данные о пользователе, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldCreateUser_WhenCommandValid()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new PutUserCommand(AdminUserAccount.Login)
		{
			BirthDay = new DateTime(2007, 01, 01),
			Gender = Gender.Female,
			Name = AdminUserAccount.User!.Name + "New",
		};

		var handler = new PutUserCommandHandler(context, AuthorizationService);
		await handler.Handle(command, default);

		var adminUserAccount = context.UserAccounts
			.Include(x => x.User)
			.FirstOrDefault(x => x.Id == AdminUserAccount.Id);

		adminUserAccount.Should().NotBeNull();
		adminUserAccount!.User!.BirthDay.Should().Be(command.BirthDay);
		adminUserAccount!.User!.Gender.Should().Be(command.Gender);
		adminUserAccount!.User!.Name.Should().Be(command.Name);
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		var command = new PutUserCommand("NotExistingLogin");

		var handler = new PutUserCommandHandler(context, AuthorizationService);
		var handle = async () => await handler.Handle(command, default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}
}
