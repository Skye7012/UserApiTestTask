using System.Text;
using HostInitActions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Infrastructure.InitExecutors;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.Infrastructure.Services;

namespace UserApiTestTask.Infrastructure;

/// <summary>
/// Конфигуратор сервисов
/// </summary>
public static class InfrastructureServicesConfigurator
{
	/// <summary>
	/// Сконфигурировать сервисы
	/// </summary>
	/// <param name="builder">Билдер приложения</param>
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, ConfigurationManager configurationManager)
		=> services
			.AddAuthorization(configurationManager)
			.AddDatabase(configurationManager)
			.AddInitExecutors();

	/// <summary>
	/// Сконфигурировать сервисы авторизации
	/// </summary>
	/// <param name="services">Сервисы</param>
	/// <param name="configurationManager">Менеджер конфигурации приложения</param>
	private static IServiceCollection AddAuthorization(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		services.AddTransient<IUserService, UserService>();

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
						.GetBytes(configurationManager.GetSection("AppSettings:Token").Value!)),
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
				});

		services.AddAuthorization(o =>
		{
			o.AddPolicy(
				CustomPolicies.IsAdminClaimPolicy,
				p => p.RequireClaim(CustomClaims.IsAdminClaimName, (true).ToString()));
		});

		return services;
	}

	/// <summary>
	/// Сконфигурировать подключение к БД
	/// </summary>
	/// <param name="services">Сервисы</param>
	/// <param name="configurationManager">Менеджер конфигурации приложения</param>
	private static IServiceCollection AddDatabase(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		var connString = configurationManager.GetConnectionString("Db")!;
		services.AddDbContext<ApplicationDbContext>(opt =>
			{
				opt.UseNpgsql(connString);
				opt.UseSnakeCaseNamingConvention();
			});

		return services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
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
