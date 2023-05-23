using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.UnitTests.Mocks;
using UserApiTestTask.UnitTests.Mocks.CacheRepositories;

namespace UserApiTestTask.UnitTests;

/// <summary>
/// Базовый класс для unit тестов
/// </summary>
public class UnitTestBase
{
	/// <summary>
	/// Конструктор
	/// </summary>
	protected UnitTestBase()
	{
		DateTimeProvider = ConfigureDateTimeProvider();
		TokenService = new TokenServiceSubstitute().Create();
		PasswordService = new PasswordServiceSubstitute().Create();
		UserAccountCacheRepository = new UserAccountCacheRepositorySubstitute().Create();
		AuthorizationService = new AuthorizationServiceSubstitute().Create();


		PasswordService.CreatePasswordHash("AdminPassword", out var passwordHash, out var passwordSalt);
		PasswordService.ClearReceivedCalls();
		AdminUserAccount = new TestUserAccount
		{
			Id = Guid.NewGuid(),
			Login = "Admin",
			Password = "AdminPassword",
			PasswordHash = passwordHash,
			PasswordSalt = passwordSalt,
			CreatedBy = "Admin",
			CreatedOn = DateTimeProvider.UtcNow,
			ModifiedBy = "Admin",
			ModifiedOn = DateTimeProvider.UtcNow,
			User = new User
			{
				Id = Guid.NewGuid(),
				Name = "Admin",
				Gender = Gender.Male,
				BirthDay = DateTimeProvider.UtcNow,
				IsAdmin = true,
				CreatedBy = "Admin",
				CreatedOn = DateTimeProvider.UtcNow,
				ModifiedBy = "Admin",
				ModifiedOn = DateTimeProvider.UtcNow,
			}
		};


		RefreshTokenValidator = new RefreshTokenValidatorSubstitute(AdminUserAccount).Create();
	}

	/// <summary>
	/// Сервис авторизации для тестов
	/// </summary>
	protected IAuthorizationService AuthorizationService { get; private set; }

	/// <summary>
	/// Сервис JWT токенов для тестов
	/// </summary>
	protected ITokenService TokenService { get; }

	/// <summary>
	/// Валидатор Refresh токенов для тестов
	/// </summary>
	protected IRefreshTokenValidator RefreshTokenValidator { get; }

	/// <summary>
	/// Сервис паролей для тестов
	/// </summary>
	protected IPasswordService PasswordService { get; }

	/// <summary>
	/// Провайдер времени
	/// </summary>
	protected IDateTimeProvider DateTimeProvider { get; }

	/// <summary>
	/// Репозиторий кэширования аккаунта пользователя
	/// </summary>
	protected IUserAccountCacheRepository UserAccountCacheRepository { get; }

	/// <summary>
	/// Аккаунт пользователь-администратора, который присутствует в тестах в БД по дефолту
	/// </summary>
	public TestUserAccount AdminUserAccount { get; }

	/// <summary>
	/// Создать контекст БД
	/// </summary>
	/// <returns>Контекст БД</returns>
	protected async Task<IApplicationDbContext> CreateInMemoryContextAsync(Func<IApplicationDbContext, Task>? seedActions = null)
	{
		var dbId = Guid.NewGuid().ToString();
		var createContext = () => new ApplicationDbContext(
			new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(dbId)
				.ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
				.Options,
			AuthorizationService,
			DateTimeProvider,
			UserAccountCacheRepository);

		var context = createContext();

		await InitializeDBAsync(context);

		if (seedActions != null)
			await seedActions.Invoke(context);

		await context.SaveChangesAsync();

		AuthorizationService = new AuthorizationServiceSubstitute(AdminUserAccount).Create();

		return createContext();
	}

	/// <summary>
	/// Создать контекст БД
	/// </summary>
	/// <returns>Контекст БД</returns>
	protected async Task<IApplicationDbContext> CreateInMemoryContextAsync(Action<IApplicationDbContext>? seedActions = null)
	{
		var asyncSeedActions = (IApplicationDbContext context) =>
		{
			seedActions?.Invoke(context);
			return Task.CompletedTask;
		};

		return await CreateInMemoryContextAsync(asyncSeedActions);
	}

	/// <summary>
	/// Создать контекст БД
	/// </summary>
	/// <returns>Контекст БД</returns>
	protected async Task<IApplicationDbContext> CreateInMemoryContextAsync()
		=> await CreateInMemoryContextAsync(null);

	/// <summary>
	/// Инициализировать БД начальными константными сущностями и пользователем-администратором
	/// </summary>
	/// <param name="context">Контекст БД</param>
	private async Task InitializeDBAsync(ApplicationDbContext context)
	{
		await context.UserAccounts.AddAsync(AdminUserAccount);
		await context.SaveChangesAsync();
	}

	/// <summary>
	/// Сконфигурировать <see cref="DateTimeProvider"/>
	/// </summary>
	private static IDateTimeProvider ConfigureDateTimeProvider()
	{
		var substitute = Substitute.For<IDateTimeProvider>();

		substitute.UtcNow
			.Returns(DateTime.SpecifyKind(
				new DateTime(2020, 01, 01),
				DateTimeKind.Utc));

		return substitute;
	}
}
