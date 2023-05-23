using System.Threading.Tasks;
using FluentAssertions;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Users.Queries.GetUser;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.UserRequests;

/// <summary>
/// Тест для <see cref="GetUserQueryHandler"/>
/// </summary>
public class GetUserQueryHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен получить информацию о пользователе, если они существует
	/// </summary>
	[Fact]
	public async Task Handle_ShouldReturnUserInfo_IfHeExists()
	{
		using var context = await CreateInMemoryContextAsync();

		var handler = new GetUserQueryHandler(context, AuthorizationService);
		var result = await handler.Handle(new GetUserQuery(AdminUserAccount.Login), default);

		result.Should().NotBeNull();

		result.Name.Should().Be(AdminUserAccount.User!.Name);
		result.Gender.Should().Be(AdminUserAccount.User!.Gender);
		result.BirthDay.Should().Be(AdminUserAccount.User!.BirthDay);
		result.IsActive.Should().Be(AdminUserAccount.User!.RevokedOn == null);
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда пользователь не найден
	/// </summary>
	[Fact]
	public async Task Handle_ShouldThrow_WhenUserNotFound()
	{
		using var context = await CreateInMemoryContextAsync();

		var handler = new GetUserQueryHandler(context, AuthorizationService);
		var handle = async () => await handler.Handle(new GetUserQuery("NotExistingLogin"), default);

		await handle.Should()
			.ThrowAsync<UserNotFoundProblem>();
	}
}
