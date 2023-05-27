using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.IntegrationTests.Extensions;
using Xunit;

namespace UserApiTestTask.IntegrationTests;

/// <summary>
/// Фабрика приложения для интеграционных тестов
/// </summary>
/// <typeparam name="TProgram">Точка входа</typeparam>
/// <typeparam name="TDbContext">Контекст БД</typeparam>
public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
	where TProgram : class where TDbContext : DbContext
{
	private readonly PostgreSqlContainer _dbContainer;
	private readonly RedisContainer _redisContainer;

	/// <summary>
	/// Конструктор
	/// </summary>
	public IntegrationTestFactory()
	{
		_dbContainer = new PostgreSqlBuilder()
			.WithName("test_db")
			.WithDatabase("test_db")
			.WithUsername("postgres")
			.WithPassword("postgres")
			.WithImage("postgres:14")
			.WithCleanUp(true)
			.Build();

		_redisContainer = new RedisBuilder()
			.WithName("test_minio")
			.WithCleanUp(true)
			.Build();
	}

	/// <summary>
	/// Подключение к БД
	/// </summary>
	public DbConnection DbConnection { get; private set; } = default!;

	/// <inheritdoc/>
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			services.RemoveDbContext<TDbContext>();
			services.AddDbContext<TDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

			services.RemoveAll<IDistributedCache>();
			services.AddStackExchangeRedisCache(opt => opt.Configuration = _redisContainer.GetConnectionString());

			services.RemoveAll<IDateTimeProvider>();
			services.AddSingleton(GetDateTimeProviderMock());
		});

		builder.UseEnvironment(Environments.Staging);
	}

	/// <inheritdoc/>
	public async Task InitializeAsync()
	{
		await _dbContainer.StartAsync();
		DbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
		await DbConnection.OpenAsync();

		await _redisContainer.StartAsync();
	}

	/// <inheritdoc/>
	public new async Task DisposeAsync()
	{
		await _dbContainer.DisposeAsync();
		await _redisContainer.DisposeAsync();
	}

	/// <summary>
	/// Получить мок <see cref="IDateTimeProvider"/>
	/// </summary>
	/// <returns>Мок <see cref="IDateTimeProvider"/></returns>
	private static IDateTimeProvider GetDateTimeProviderMock()
	{
		var dateTimeProvider = Substitute.For<IDateTimeProvider>();

		dateTimeProvider.UtcNow
			.Returns(DateTime.SpecifyKind(
				new DateTime(2020, 01, 01),
				DateTimeKind.Utc));

		return dateTimeProvider;
	}
}
