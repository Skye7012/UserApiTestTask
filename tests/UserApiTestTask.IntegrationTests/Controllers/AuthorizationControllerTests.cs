using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Api.Controllers;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserLogin;
using UserApiTestTask.Contracts.Requests.Authorization.PutUserPassword;
using UserApiTestTask.Contracts.Requests.Authorization.Refresh;
using UserApiTestTask.Contracts.Requests.Authorization.SignIn;
using UserApiTestTask.Contracts.Requests.Authorization.SignUp;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.IntegrationTests.Extensions;
using Xunit;

namespace UserApiTestTask.IntegrationTests.Controllers;

/// <summary>
/// Тесты для <see cref="AuthorizationController"/>
/// </summary>
public class AuthorizationControllerTests : IntegrationTestsBase
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="factory">Фабрика приложения</param>
	public AuthorizationControllerTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : base(factory)
	{
	}

	/// <summary>
	/// Должен создать пользователя, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task SignUpAsync_ShouldCreateUser_WhenRequestValid()
	{
		Authenticate();

		var request = new SignUpRequest
		{
			Login = "Login",
			Name = "Name",
			Password = "Password",
			BirthDay = DateTimeProvider.UtcNow,
			Gender = Gender.Male,
			IsAdmin = false,
		};

		var response = await Client.SignUpAsync(request);

		response.Result.Should().NotBeNull();

		var createdUser = await DbContext.Users
			.Include(x => x.UserAccount)
			.FirstOrDefaultAsync(x => x.Id == response.Result!.UserId);

		createdUser.Should().NotBeNull();
		createdUser!.UserAccount!.Should().NotBeNull();

		createdUser!.Name.Should().Be(request.Name);
		createdUser!.BirthDay.Should().Be(request.BirthDay);
		createdUser!.Gender.Should().Be(request.Gender);
		createdUser!.IsAdmin.Should().Be(request.IsAdmin);
		createdUser!.UserAccount!.Login.Should().Be(request.Login);

		PasswordService
			.VerifyPasswordHash(
				"Password",
				createdUser.UserAccount!.PasswordHash,
				createdUser.UserAccount!.PasswordSalt)
			.Should().BeTrue();
	}

	/// <summary>
	/// Должен создать правильные токены, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task SignInAsync_ShouldCreateValidTokens_WhenRequestValid()
	{
		var request = new SignInRequest
		{
			Login = Seeder.AdminUserAccount.Login,
			Password = Seeder.AdminUserAccount.Password,
		};

		var result = (await Client
			.SignInAsync(request))
				.Result!;

		result.Should().NotBeNull();
		result.AccessToken.Should().NotBeNullOrWhiteSpace();
		result.RefreshToken.Should().NotBeNullOrWhiteSpace();

		ValidateTokenClaims(result.AccessToken, result.RefreshToken);

		var isRefreshTokenWasCreatedInDb = await DbContext.RefreshTokens
			.AnyAsync(x => x.Token == result.RefreshToken);

		isRefreshTokenWasCreatedInDb.Should().BeTrue();
	}

	/// <summary>
	/// Должен обновить токены, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task RefreshAsync_ShouldRefreshTokens_WhenRequestValid()
	{
		Authenticate();

		var signInRequest = new SignInRequest
		{
			Login = Seeder.AdminUserAccount.Login,
			Password = Seeder.AdminUserAccount.Password,
		};

		var signInResponse = await Client
			.SignInAsync(signInRequest);

		var request = new RefreshTokenRequest
		{
			RefreshToken = signInResponse.Result!.RefreshToken,
		};

		var response = (await Client.RefreshTokensAsync(request))
			.Result!;

		response.Should().NotBeNull();
		response.AccessToken.Should().NotBeNullOrWhiteSpace();
		response.RefreshToken.Should().NotBeNullOrWhiteSpace();

		ValidateTokenClaims(response.AccessToken, response.RefreshToken);

		var isRefreshTokenWasCreatedInDb = await DbContext.RefreshTokens
			.AnyAsync(x => x.Token == response.RefreshToken);

		isRefreshTokenWasCreatedInDb.Should().BeTrue();

		var isOldRefreshTokenWasRevoked = await DbContext.RefreshTokens
			.AnyAsync(x => x.Token == signInResponse.Result.RefreshToken && x.RevokedOn != null);
	}

	/// <summary>
	/// Должен деактивировать все refresh токены, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task SignOutAsync_ShouldRevokeAllRefreshTokens_WhenRequestValid()
	{
		Authenticate();

		for (int i = 0; i < 3; i++)
		{
			var signInRequest = new SignInRequest
			{
				Login = Seeder.AdminUserAccount.Login,
				Password = Seeder.AdminUserAccount.Password,
			};

			var signInResponse = await Client.SignInAsync(signInRequest);
		}

		var response = await Client.SignOutAsync();

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		DbContext.Instance.ChangeTracker.Clear();
		var refreshTokens = await DbContext.RefreshTokens
			.Where(x => x.UserAccountId == Seeder.AdminUserAccount.Id)
			.ToListAsync();

		refreshTokens.Count.Should().Be(3);

		refreshTokens.Should().OnlyContain(x => x.RevokedOn != null);
	}

	/// <summary>
	/// Должен обновить пароль аккаунта пользователя, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task PutPasswordAsync_ShouldUpdateUser_WhenRequestValid()
	{
		Authenticate();

		for (int i = 0; i < 3; i++)
		{
			var signInRequest = new SignInRequest
			{
				Login = Seeder.AdminUserAccount.Login,
				Password = Seeder.AdminUserAccount.Password,
			};

			var signInResponse = await Client.SignInAsync(signInRequest);
		}

		var request = new PutUserPasswordRequest
		{
			NewPassword = Seeder.AdminUserAccount.Password + "New",
			OldPassword = Seeder.AdminUserAccount.Password,
		};

		var response = await Client.ChangePasswordAsync(
			Seeder.AdminUserAccount.Login,
			request);

		response.Result!.Should().NotBeNull();
		response.Result!.AccessToken.Should().NotBeNullOrEmpty();
		response.Result!.RefreshToken.Should().NotBeNullOrEmpty();

		ValidateTokenClaims(
			response.Result.AccessToken,
			response.Result.RefreshToken);

		DbContext.Instance.ChangeTracker.Clear();
		var updatedUser = await DbContext.UserAccounts
			.FirstAsync(x => x.Id == Seeder.AdminUserAccount.Id);

		PasswordService.VerifyPasswordHash(
				request.NewPassword,
				updatedUser!.PasswordHash,
				updatedUser!.PasswordSalt)
			.Should().BeTrue();

		DbContext.Instance.ChangeTracker.Clear();
		var refreshTokens = await DbContext.RefreshTokens
			.Where(x => x.UserAccountId == Seeder.AdminUserAccount.Id)
			.ToListAsync();

		refreshTokens.Count.Should().Be(4);
		refreshTokens.Where(x => x.RevokedOn != null).Count()
			.Should().Be(3);

		refreshTokens.First(x => x.RevokedOn == null)
			.Token
			.Should().Be(response.Result.RefreshToken);
	}

	/// <summary>
	/// Должен обновить логин аккаунта пользователя, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task PutLoginAsync_ShouldUpdateUser_WhenRequestValid()
	{
		Authenticate();

		for (int i = 0; i < 3; i++)
		{
			var signInRequest = new SignInRequest
			{
				Login = Seeder.AdminUserAccount.Login,
				Password = Seeder.AdminUserAccount.Password,
			};

			var signInResponse = await Client.SignInAsync(signInRequest);
		}

		var login = await UserAccountCacheRepository.GetLoginAsync(Seeder.AdminUserAccount.Id, default);
		login.Should().Be(Seeder.AdminUserAccount.Login);

		var request = new PutUserLoginRequest
		{
			NewLogin = Seeder.AdminUserAccount.Login + "New",
		};

		var response = await Client.ChangeLoginAsync(
			Seeder.AdminUserAccount.Login,
			request);

		response.Result!.Should().NotBeNull();
		response.Result!.AccessToken.Should().NotBeNullOrEmpty();
		response.Result!.RefreshToken.Should().NotBeNullOrEmpty();

		ValidateTokenClaims(
			response.Result.AccessToken,
			response.Result.RefreshToken);

		DbContext.Instance.ChangeTracker.Clear();
		var updatedUser = await DbContext.UserAccounts
			.FirstAsync(x => x.Id == Seeder.AdminUserAccount.Id);

		updatedUser.Login.Should().Be(request.NewLogin);

		DbContext.Instance.ChangeTracker.Clear();
		var refreshTokens = await DbContext.RefreshTokens
			.Where(x => x.UserAccountId == Seeder.AdminUserAccount.Id)
			.ToListAsync();

		refreshTokens.Count.Should().Be(4);
		refreshTokens.Where(x => x.RevokedOn != null).Count()
			.Should().Be(3);

		refreshTokens.First(x => x.RevokedOn == null)
			.Token
			.Should().Be(response.Result.RefreshToken);

		login = await UserAccountCacheRepository.GetLoginAsync(Seeder.AdminUserAccount.Id, default);
		login.Should().Be(request.NewLogin);
	}

	/// <summary>
	/// Провалидировать клеймы токенов
	/// </summary>
	private void ValidateTokenClaims(string accessToken, string refreshToken)
	{
		var jwtAccessToken = new JwtSecurityToken(accessToken);

		var userIdClaimValue = jwtAccessToken.Claims
			.First(x => x.Type == CustomClaims.UserIdСlaimName)
			.Value;

		userIdClaimValue.Should().Be(Seeder.AdminUserAccount.User!.Id.ToString());

		var userAccountIdClaimValue = jwtAccessToken.Claims
			.First(x => x.Type == CustomClaims.UserAccountIdClaimName)
			.Value;

		userAccountIdClaimValue.Should().Be(Seeder.AdminUserAccount.Id.ToString());

		var isAdminClaimValue = bool.Parse(jwtAccessToken.Claims
			.First(x => x.Type == CustomClaims.IsAdminClaimName)
			.Value);

		isAdminClaimValue.Should().Be(Seeder.AdminUserAccount.User!.IsAdmin);

		jwtAccessToken.ValidTo.Should().BeCloseTo(
			DateTime.UtcNow.AddSeconds(JwtConfig.AccessTokenLifeTime),
			TimeSpan.FromSeconds(1));


		var jwtRefreshToken = new JwtSecurityToken(refreshToken);

		jwtRefreshToken.ValidTo.Should().BeCloseTo(
			DateTime.UtcNow.AddSeconds(JwtConfig.RefreshTokenLifeTime),
			TimeSpan.FromSeconds(1));
	}
}
