using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using UserApiTestTask.Application.Users.Queries.GetActiveUsers;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="GetActiveUsersQueryHandler"/>
/// </summary>
public class GetActiveUsersQueryHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен получить информацию о пользователях, которые активны
	/// </summary>
	[Fact]
	public async Task Handle_ShouldReturnUserInfo_WhoIsActive()
	{
		var userAccountToRemove = new UserAccount
		{
			Login = "ToRemove",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = false,
				Name = "ToRemove",
				Gender = Gender.Unknown,
			}
		};

		using var context = await CreateInMemoryContextAsync(x =>
		{
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
			});

			x.UserAccounts.Add(userAccountToRemove);
		});

		context.UserAccounts.Remove(userAccountToRemove);
		await context.SaveChangesAsync();

		var handler = new GetActiveUsersQueryHandler(context, AuthorizationService);
		var result = await handler.Handle(new GetActiveUsersQuery(), default);

		result.Should().NotBeNull();
		result.TotalCount.Should().Be(2);

		result.Items.Should().NotBeNullOrEmpty();

		result.Items.Should().NotContain(x => x.Name == userAccountToRemove.User.Name);

		var adminUser = result.Items!
			.First(x => x.Name == AdminUserAccount.User!.Name);

		adminUser.BirthDay.Should().Be(AdminUserAccount.User!.BirthDay);
		adminUser.Gender.Should().Be(AdminUserAccount.User!.Gender);
		adminUser.IsActive.Should().Be(true);

		AuthorizationService
			.Received(1)
			.CheckIsAdmin();
	}
}
