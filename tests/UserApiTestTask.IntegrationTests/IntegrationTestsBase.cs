using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Respawn;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Infrastructure.Configs;
using UserApiTestTask.Infrastructure.Persistence;
using Xunit;

namespace UserApiTestTask.IntegrationTests;

/// <summary>
/// Базовый класс для интеграционных тестов
/// </summary>
[Collection("FactoryCollection")]
public class IntegrationTestsBase : IAsyncLifetime
{
	protected readonly IntegrationTestFactory<Program, ApplicationDbContext> _factory;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="factory">Фабрика приложения</param>
	protected IntegrationTestsBase(IntegrationTestFactory<Program, ApplicationDbContext> factory)
	{
		_factory = factory;
		Client = factory.CreateClient();
		DbContext = factory.Services.CreateScope()
			.ServiceProvider.GetRequiredService<IApplicationDbContext>();

		AuthorizationService = factory.Services.GetRequiredService<IAuthorizationService>();
		TokenService = factory.Services.GetRequiredService<ITokenService>();
		RefreshTokenValidator = factory.Services.GetRequiredService<IRefreshTokenValidator>();
		PasswordService = factory.Services.GetRequiredService<IPasswordService>();

		UserAccountCacheRepository = factory.Services.GetRequiredService<IUserAccountCacheRepository>();

		DateTimeProvider = factory.Services.GetRequiredService<IDateTimeProvider>();
		JwtConfig = factory.Services.GetRequiredService<IOptions<JwtConfig>>()
			.Value;

		Seeder = new IntegrationTestSeeder(DbContext, PasswordService, DateTimeProvider);
	}

	/// <summary>
	/// Http клиент
	/// </summary>
	protected HttpClient Client { get; }

	/// <summary>
	/// Контекст БД
	/// </summary>
	protected IApplicationDbContext DbContext { get; }

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
	/// Сидер
	/// </summary>
	protected IntegrationTestSeeder Seeder { get; }

	/// <summary>
	/// Respawner БД
	/// </summary>
	protected Respawner Respawner { get; private set; } = default!;

	/// <summary>
	/// Провайдер времени
	/// </summary>
	public IDateTimeProvider DateTimeProvider { get; }

	/// <summary>
	/// Репозиторий кэширования аккаунта пользователя
	/// </summary>
	protected IUserAccountCacheRepository UserAccountCacheRepository { get; }

	/// <summary>
	/// Конфигурация для JWT
	/// </summary>
	protected JwtConfig JwtConfig { get; }

	/// <inheritdoc/>
	public async Task DisposeAsync()
		=> await Respawner.ResetAsync(_factory.DbConnection);

	/// <inheritdoc/>
	public async Task InitializeAsync()
	{
		await Seeder.SeedInitialDataAsync();

		Respawner = await Respawner.CreateAsync(
			_factory.DbConnection,
			new RespawnerOptions
			{
				DbAdapter = DbAdapter.Postgres,
				WithReseed = true,
			});
	}

	/// <summary>
	/// Аутентифицироваться
	/// </summary>
	/// <param name="token">Токен аутентификации</param>
	protected void Authenticate(string? token = null)
		=> Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
			"bearer",
			token ?? TokenService.CreateAccessToken(Seeder.AdminUserAccount));
}
