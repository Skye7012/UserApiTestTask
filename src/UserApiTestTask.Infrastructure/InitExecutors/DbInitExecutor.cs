using HostInitActions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Persistence;

namespace UserApiTestTask.Infrastructure.InitExecutors;

/// <summary>
/// Инициализатор базы данных
/// </summary>
public class DbInitExecutor : IAsyncInitActionExecutor
{
	/// <summary>
	/// Логин пользователя-администратора, который создается автоматически
	/// </summary>
	public const string AdminLogin = "Admin";

	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IWebHostEnvironment _environment;
	private readonly IConfiguration _configuration;
	private readonly IPasswordService _passwordService;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="scopeFactory">Фабрика скоупов</param>
	/// <param name="environment">Переменные среды</param>
	/// <param name="configuration">Конфигурация приложения</param>
	/// <param name="passwordService">Сервис паролей</param>
	public DbInitExecutor(
		IServiceScopeFactory scopeFactory,
		IWebHostEnvironment environment,
		IConfiguration configuration,
		IPasswordService passwordService)
	{
		_scopeFactory = scopeFactory;
		_environment = environment;
		_configuration = configuration;
		_passwordService = passwordService;
	}

	/// <summary>
	/// Провести инициализацию БД
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	public async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		if (_environment.IsStaging())
		{
			await db.Database.EnsureCreatedAsync(cancellationToken);
			return;
		}

		await db.Database.MigrateAsync(cancellationToken);



		var isAdminExists = await db.UserAccounts
			.AnyAsync(u => u.Login == AdminLogin, cancellationToken);

		if (!isAdminExists)
		{
			_passwordService.CreatePasswordHash(
				_configuration["AppSettings:AdminPassword"],
				out var passwordHash,
				out var passwordSalt);

			await db.Users.AddAsync(new User
			{
				Name = "Admin",
				BirthDay = GetDate(2000),
				Gender = Gender.Male,
				IsAdmin = true,
				CreatedBy = "Admin",
				CreatedOn = GetDate(2020),
				ModifiedBy = "Admin",
				ModifiedOn = GetDate(2020),
				UserAccount = new UserAccount
				{
					Login = AdminLogin,
					PasswordHash = passwordHash,
					PasswordSalt = passwordSalt,
				}
			},
			cancellationToken);

			await db.SaveChangesAsync(cancellationToken);
		}
	}

	/// <summary>
	/// Получить дату
	/// </summary>
	/// <param name="year">Год</param>
	public static DateTime GetDate(int year)
		=> DateTime.SpecifyKind(
			new DateTime(year, 01, 01),
			DateTimeKind.Utc);
}
