using System;
using System.Threading.Tasks;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.IntegrationTests.Mocks;

namespace UserApiTestTask.IntegrationTests;

/// <summary>
/// Сидер для интеграционных тестов
/// </summary>
public class IntegrationTestSeeder
{
	private readonly IApplicationDbContext _context;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="userService">Сервис пользователя</param>
	public IntegrationTestSeeder(
		IApplicationDbContext dbContext,
		IPasswordService passwordService,
		IDateTimeProvider dateTimeProvider)
	{
		_context = dbContext;

		passwordService.CreatePasswordHash("AdminPassword", out var passwordHash, out var passwordSalt);

		AdminUserAccount = new TestUserAccount
		{
			Id = Guid.NewGuid(),
			Login = "Admin",
			Password = "AdminPassword",
			PasswordHash = passwordHash,
			PasswordSalt = passwordSalt,
			CreatedBy = "Admin",
			CreatedOn = dateTimeProvider.UtcNow,
			ModifiedBy = "Admin",
			ModifiedOn = dateTimeProvider.UtcNow,
			User = new User
			{
				Id = Guid.NewGuid(),
				Name = "Admin",
				Gender = Gender.Male,
				BirthDay = dateTimeProvider.UtcNow.AddYears(-30),
				IsAdmin = true,
				CreatedBy = "Admin",
				CreatedOn = dateTimeProvider.UtcNow,
				ModifiedBy = "Admin",
				ModifiedOn = dateTimeProvider.UtcNow,
			}
		};
	}

	/// <summary>
	/// Администратор
	/// </summary>
	public TestUserAccount AdminUserAccount { get; }

	/// <summary>
	/// Проинициализировать БД начальными данными, которые должны быть в каждом тесте
	/// </summary>
	public async Task SeedInitialDataAsync()
	{
		_context.Instance.ChangeTracker.Clear();

		await _context.UserAccounts.AddAsync(AdminUserAccount);
		await _context.SaveChangesAsync();
	}
}
