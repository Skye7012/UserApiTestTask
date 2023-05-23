using HostInitActions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserApiTestTask.Application.Common.Extensions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Infrastructure.Configs;
using UserApiTestTask.Infrastructure.InitExecutors;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.Infrastructure.Services;
using UserApiTestTask.Infrastructure.Services.CacheRepositories;

namespace UserApiTestTask.Infrastructure;

/// <summary>
/// Конфигуратор сервисов
/// </summary>
public static class InfrastructureServicesConfigurator
{
	/// <summary>
	/// Сконфигурировать сервисы
	/// </summary>
	/// <param name="services">Билдер приложения</param>
	/// <param name="configuration">Конфигурации приложения</param>
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		=> services
			.AddAuthorization(configuration)
			.AddDatabase(configuration)
			.AddRedis(configuration)
			.AddInitExecutors();

	/// <summary>
	/// Сконфигурировать сервисы авторизации
	/// </summary>
	/// <param name="services">Сервисы</param>
	/// <param name="configuration">Конфигурации приложения</param>
	private static IServiceCollection AddAuthorization(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddTransient<IPasswordService, PasswordService>()
			.AddTransient<ITokenService, TokenService>()
			.AddTransient<IRefreshTokenValidator, RefreshTokenValidator>()
			.AddTransient<IAuthorizationService, AuthorizationService>();

		var jwtConfig = services.ConfigureAndGet<JwtConfig>(
			configuration,
			JwtConfig.ConfigSectionName);

		var tokenValidationParameters = jwtConfig.BuildTokenValidationParameters();

		services
			.AddSingleton(tokenValidationParameters)
			.AddTransient<IRefreshTokenValidator, RefreshTokenValidator>();

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options => options.TokenValidationParameters = tokenValidationParameters);

		services.AddAuthorization();

		return services;
	}

	/// <summary>
	/// Сконфигурировать подключение к БД
	/// </summary>
	/// <param name="services">Сервисы</param>
	/// <param name="configuration">Конфигурации приложения</param>
	private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connString = configuration.GetConnectionString("Db")!;
		services.AddDbContext<ApplicationDbContext>(opt =>
			{
				opt.UseNpgsql(connString);
				opt.UseSnakeCaseNamingConvention();
			});

		return services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
	}

	/// <summary>
	/// Сконфигурировать Redis
	/// </summary>
	/// <param name="services">Сервисы</param>
	/// <param name="configuration">Конфигурации приложения</param>
	private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
	{
		var connString = configuration.GetConnectionString("Redis")!;
		return services.AddStackExchangeRedisCache(opt => opt.Configuration = connString)
			.AddTransient<IUserAccountCacheRepository, UserAccountCacheRepository>();
	}

	/// <summary>
	/// Сконфигурировать инициализаторы сервисов
	/// </summary>
	/// <param name="services">Сервисы</param>
	private static IServiceCollection AddInitExecutors(this IServiceCollection services)
	{
		services.AddAsyncServiceInitialization()
			.AddInitActionExecutor<DbInitExecutor>();

		return services;
	}
}
