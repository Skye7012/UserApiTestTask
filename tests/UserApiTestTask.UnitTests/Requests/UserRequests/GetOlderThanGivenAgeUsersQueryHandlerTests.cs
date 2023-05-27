using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using UserApiTestTask.Application.Users.Queries.GetOlderThanGivenAgeUsers;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="GetOlderThanGivenAgeUsersQueryHandler"/>
/// </summary>
public class GetOlderThanGivenAgeUsersQueryHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен получить информацию о пользователях, которые старше 8 лет
	/// </summary>
	[Fact]
	public async Task Handle_ShouldReturnUserInfo_WhoAreOver8YearsOld()
	{
		using var context = await CreateInMemoryContextAsync(x =>
		{
			x.UserAccounts.Add(new UserAccount
			{
				Login = "new",
				PasswordHash = new byte[] { 1, 2 },
				PasswordSalt = new byte[] { 3, 4 },
				User = new User
				{
					BirthDay = new DateTime(2018, 01, 01),
					IsAdmin = false,
					Name = "newUser",
					Gender = Gender.Unknown,
				}
			});

			x.UserAccounts.Add(new UserAccount
			{
				Login = "new2",
				PasswordHash = new byte[] { 1, 2 },
				PasswordSalt = new byte[] { 3, 4 },
				User = new User
				{
					BirthDay = new DateTime(2015, 01, 01),
					IsAdmin = false,
					Name = "newUser",
					Gender = Gender.Unknown,
				}
			});

			AdminUserAccount.User!.BirthDay = new DateTime(2010, 01, 01);
		});

		var handler = new GetOlderThanGivenAgeUsersQueryHandler(context, DateTimeProvider, AuthorizationService);
		var result = await handler.Handle(new GetOlderThanGivenAgeUsersQuery(8), default);

		result.Should().NotBeNull();
		result.TotalCount.Should().Be(1);

		result.Items.Should().NotBeNullOrEmpty();

		var adminUser = result.Items!.Single();

		adminUser.BirthDay.Should().Be(AdminUserAccount.User!.BirthDay);
		adminUser.Gender.Should().Be(AdminUserAccount.User!.Gender);
		adminUser.IsActive.Should().Be(true);

		AuthorizationService
			.Received(1)
			.CheckIsAdmin();
	}
}
