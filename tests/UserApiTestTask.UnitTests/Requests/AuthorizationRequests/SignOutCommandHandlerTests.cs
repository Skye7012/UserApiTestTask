using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UserApiTestTask.Application.Authorization.Commands.SignOut;
using UserApiTestTask.Domain.Entities;
using Xunit;

namespace UserApiTestTask.UnitTests.Requests.AuthorizationRequests;

/// <summary>
/// Тест для <see cref="SignOutCommandHandler"/>
/// </summary>
public class SignOutCommandHandlerTests : UnitTestBase
{
	/// <summary>
	/// Должен деактивировать все refresh токены пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task Handle_ShouldSoftDeleteAllRefreshTokens_WhenCommandValid()
	{
		var refreshToken = TokenService.CreateRefreshToken();

		using var context = await CreateInMemoryContextAsync(x =>
		{
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "1", AdminUserAccount));
			x.RefreshTokens.Add(new RefreshToken(refreshToken + "2", AdminUserAccount));
		});

		var handler = new SignOutCommandHandler(
			context,
			AuthorizationService);

		await handler.Handle(new SignOutCommand(), default);

		context.Instance.ChangeTracker.Clear();

		var refreshTokenCount = context.RefreshTokens
			.Where(x => x.UserAccountId == AdminUserAccount.Id)
			.Count();

		refreshTokenCount.Should().Be(2);

		var refreshTokensWereSoftDeleted = context.RefreshTokens
			.Where(x => x.UserAccountId == AdminUserAccount.Id)
			.All(x => x.RevokedOn != null);

		refreshTokensWereSoftDeleted.Should().BeTrue();
	}
}
