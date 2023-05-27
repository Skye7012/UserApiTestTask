using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Api.Controllers;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Contracts.Requests.Users.PutUser;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.IntegrationTests.Extensions;
using Xunit;

namespace UserApiTestTask.IntegrationTests.Controllers;

/// <summary>
/// Тесты для <see cref="UserController"/>
/// </summary>
public class UserControllerTests : IntegrationTestsBase
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="factory">Фабрика приложения</param>
	public UserControllerTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : base(factory)
	{
	}

	/// <summary>
	/// Должен вернуть данные о пользователе, когда он существует
	/// </summary>
	[Fact]
	public async Task GetAsync_ShouldReturnUser_WhenUserExist()
	{
		Authenticate();

		var response = (await Client.GetUserAsync(Seeder.AdminUserAccount.Login))
			.Result;

		response.Should().NotBeNull();

		response!.Name.Should().Be(Seeder.AdminUserAccount.User!.Name);
		response!.Gender.Should().Be(Seeder.AdminUserAccount.User!.Gender);
		response!.BirthDay.Should().Be(Seeder.AdminUserAccount.User!.BirthDay);
		response!.IsActive.Should().Be(Seeder.AdminUserAccount.User!.RevokedOn == null);
	}

	/// <summary>
	/// Должен получить информацию о пользователях, которые активны
	/// </summary>
	[Fact]
	public async Task GetActiveUsersAsync_ShouldReturnUsers_WhoIsActive()
	{
		Authenticate();

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

		DbContext.UserAccounts.Add(new UserAccount
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

		DbContext.UserAccounts.Add(userAccountToRemove);

		await DbContext.SaveChangesAsync();

		DbContext.UserAccounts.Remove(userAccountToRemove);
		await DbContext.SaveChangesAsync();

		var response = (await Client.GetActiveUsersAsync())
			.Result!;

		response.Should().NotBeNull();
		response.TotalCount.Should().Be(2);

		response.Items.Should().NotBeNullOrEmpty();

		response.Items.Should().NotContain(x => x.Name == userAccountToRemove.User.Name);

		var adminUser = response.Items!
			.First(x => x.Name == Seeder.AdminUserAccount.User!.Name);

		adminUser.BirthDay.Should().Be(Seeder.AdminUserAccount.User!.BirthDay);
		adminUser.Gender.Should().Be(Seeder.AdminUserAccount.User!.Gender);
		adminUser.IsActive.Should().Be(true);
	}

	/// <summary>
	/// Должен получить информацию о пользователях, которые старше 8 лет
	/// </summary>
	[Fact]
	public async Task GetOlderThanGivenAgeUsersAsync_ShouldReturnUsers_WhoAreOver8YearsOld()
	{
		Authenticate();

		DbContext.UserAccounts.Add(new UserAccount
		{
			Login = "new",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow.AddYears(-2),
				IsAdmin = false,
				Name = "newUser",
				Gender = Gender.Unknown,
			}
		});

		DbContext.UserAccounts.Add(new UserAccount
		{
			Login = "new2",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow.AddYears(-5),
				IsAdmin = false,
				Name = "newUser",
				Gender = Gender.Unknown,
			}
		});

		Seeder.AdminUserAccount.User!.BirthDay = DateTimeProvider.UtcNow.AddYears(-10);

		await DbContext.SaveChangesAsync();

		var response = (await Client.GetGetOlderThanGivenAgeUsersAsync(8))
			.Result!;

		response.Should().NotBeNull();
		response.TotalCount.Should().Be(1);

		response.Items.Should().NotBeNullOrEmpty();

		var adminUser = response.Items!.Single();

		adminUser.BirthDay.Should().Be(Seeder.AdminUserAccount.User!.BirthDay);
		adminUser.Gender.Should().Be(Seeder.AdminUserAccount.User!.Gender);
		adminUser.IsActive.Should().Be(true);
	}

	/// <summary>
	/// Должен обновить данные о пользователе, когда запрос валиден
	/// </summary>
	[Fact]
	public async Task PutAsync_ShouldUpdateUser_WhenRequestValid()
	{
		Authenticate();

		var request = new PutUserRequest
		{
			Name = "NewName",
			Gender = Gender.Unknown,
			BirthDay = DateTimeProvider.UtcNow.AddDays(8),
		};

		var response = await Client.PutUserAsync(Seeder.AdminUserAccount.Login, request);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		DbContext.Instance.ChangeTracker.Clear();
		var updatedUser = await DbContext.Users
			.FirstAsync(x => x.Id == Seeder.AdminUserAccount.User!.Id);

		updatedUser!.Name.Should().Be(request.Name);
		updatedUser!.Gender.Should().Be(request.Gender);
		updatedUser!.BirthDay.Should().Be(request.BirthDay);
	}

	/// <summary>
	/// Должен восстановить пользователя, когда команда валидна
	/// </summary>
	[Fact]
	public async Task RestoreUserAsync_ShouldRestoreUser_WhenRequestValid()
	{
		Authenticate();

		var removedUserAccount = new UserAccount
		{
			Login = "Removed",
			PasswordHash = new byte[] { 1, 2 },
			PasswordSalt = new byte[] { 3, 4 },
			User = new User
			{
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = false,
				Name = "Removed",
				Gender = Gender.Unknown,
			}
		};

		DbContext.UserAccounts.Add(removedUserAccount);
		await DbContext.SaveChangesAsync();

		DbContext.UserAccounts.Remove(removedUserAccount);
		await DbContext.SaveChangesAsync();

		var response = await Client.RestoreUserAsync(removedUserAccount.Login);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		DbContext.Instance.ChangeTracker.Clear();
		var restoredUserAccount = await DbContext.UserAccounts
			.Include(x => x.User)
			.FirstAsync(x => x.Id == removedUserAccount.Id);

		restoredUserAccount.RevokedOn.Should().BeNull();
		restoredUserAccount.RevokedBy.Should().BeNull();
		restoredUserAccount.User!.RevokedOn.Should().BeNull();
		restoredUserAccount.User!.RevokedBy.Should().BeNull();
	}

	// <summary>
	/// Должен удалить пользователя, когда он существует
	/// </summary>
	[Fact]
	public async Task DeleteAsync_ShouldDeleteUser_WhenHeExists()
	{
		Authenticate();

		var deleteResponse = await Client.DeleteUserAsync(Seeder.AdminUserAccount.Login);

		deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

		DbContext.Instance.ChangeTracker.Clear();
		var adminUserAccount = await DbContext.UserAccounts
			.Include(x => x.User)
			.FirstAsync(x => x.Id == Seeder.AdminUserAccount.Id);

		adminUserAccount.RevokedOn.Should().Be(DateTimeProvider.UtcNow);
		adminUserAccount.RevokedBy.Should().Be(Seeder.AdminUserAccount.Login);
		adminUserAccount.User!.RevokedOn.Should().Be(DateTimeProvider.UtcNow);
		adminUserAccount.User!.RevokedBy.Should().Be(Seeder.AdminUserAccount.Login);
	}
}
