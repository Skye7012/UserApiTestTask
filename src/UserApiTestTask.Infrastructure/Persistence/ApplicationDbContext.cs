using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Static;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Entities.Common;

namespace UserApiTestTask.Infrastructure.Persistence;

/// <summary>
/// Контекст БД данного приложения
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
	private readonly IAuthorizationService _authorizationService;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IDistributedCache _cache;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="options">Опции контекста</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	/// <param name="dateTimeProvider">Провайдер даты и времени</param>
	/// <param name="cache">Сервис кэширования</param>
	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options,
		IAuthorizationService authorizationService,
		IDateTimeProvider dateTimeProvider,
		IDistributedCache cache)
		: base(options)
	{
		_authorizationService = authorizationService;
		_dateTimeProvider = dateTimeProvider;
		_cache = cache;
	}

	/// <inheritdoc/>
	public DbContext Instance => this;

	/// <summary>
	/// Пользователи
	/// </summary>
	public DbSet<User> Users { get; private set; } = default!;

	/// <summary>
	/// Аккаунты пользователей
	/// </summary>
	public DbSet<UserAccount> UserAccounts { get; private set; } = default!;

	/// <summary>
	/// Refresh токены
	/// </summary>
	public DbSet<RefreshToken> RefreshTokens { get; private set; } = default!;

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

	/// <inheritdoc/>
	public async Task<int> SaveChangesAsync(
		bool withSoftDelete = true,
		bool acceptAllChangesOnSuccess = true,
		CancellationToken cancellationToken = default)
	{
		await HandleSaveChangesLogicAsync(withSoftDelete, cancellationToken);

		return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	/// <summary>
	/// Сохранить изменения
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Количество записей состояния, записанных в базу данных</returns>
	public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await HandleSaveChangesLogicAsync(true, cancellationToken);

		return await base.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Обработать логику сохранения изменений
	/// </summary>
	/// <param name="withSoftDelete">Использовать мягкое удаление</param>
	/// <param name="cancellationToken">Токен отмены</param>
	private async Task HandleSaveChangesLogicAsync(bool withSoftDelete, CancellationToken cancellationToken)
	{
		if (!_authorizationService.IsAuthenticated())
		{
			HandleRefreshTokenAddingAndRemovingWhenSignIn();
			return;
		}

		var willLoginBeNeeded = ChangeTracker.Entries<EntityBase>()?.Any() == true
			|| ChangeTracker.Entries<ISoftDeletable>()?.Any() == true;

		if (!willLoginBeNeeded)
			return;

		var login = await GetCachedLoginAsync(cancellationToken);

		HandleEntityBaseCreatedAndModifiedMetadata(login);

		if (!withSoftDelete)
			return;

		HandleSoftDelete(login);
	}

	/// <summary>
	/// Обработать refresh токены при аутентификации
	/// (их добавлении и удалении при привышении лимита)
	/// </summary>
	private void HandleRefreshTokenAddingAndRemovingWhenSignIn()
	{
		var refreshTokens = ChangeTracker.Entries<RefreshToken>();

		if (refreshTokens?.Any() == true)
		{
			var refreshTokenToAdd = refreshTokens
				.Where(c => c.State == EntityState.Added)
				.SingleOrDefault();

			if (refreshTokenToAdd is null)
				return;

			var login = refreshTokenToAdd.Entity.UserAccount!.Login;

			refreshTokenToAdd.Entity.CreatedOn = _dateTimeProvider.UtcNow;
			refreshTokenToAdd.Entity.CreatedBy = login;
			refreshTokenToAdd.Entity.ModifiedOn = _dateTimeProvider.UtcNow;
			refreshTokenToAdd.Entity.ModifiedBy = login;

			var refreshTokenToRemove = refreshTokens
				.Where(c => c.State == EntityState.Deleted)
				.SingleOrDefault();

			if (refreshTokenToRemove is not null)
			{
				refreshTokenToRemove.Entity.RevokedOn = _dateTimeProvider.UtcNow;
				refreshTokenToRemove.Entity.RevokedBy = login;
				refreshTokenToRemove.State = EntityState.Modified;
			}
		}
	}

	/// <summary>
	/// Получить закешированный логин
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Закешированный логин</returns>
	private async Task<string> GetCachedLoginAsync(CancellationToken cancellationToken)
	{
		var login = await _cache.GetStringAsync(
			RedisKeys.GetUserAccountLoginKey(_authorizationService.GetUserAccountId()),
			cancellationToken);

		if (login == null)
		{
			login = (await UserAccounts
				.FirstOrDefaultAsync(x => _authorizationService.GetUserAccountId() == x.Id, cancellationToken))
					?.Login
				?? throw new EntityNotFoundProblem<UserAccount>(_authorizationService.GetUserId());

			await _cache.SetStringAsync(
				RedisKeys.GetUserAccountLoginKey(_authorizationService.GetUserAccountId()),
				login,
				cancellationToken);
		}

		return login;
	}

	/// <summary>
	/// Обработать метаданные добавления и обновления у базовых сущностей
	/// </summary>
	/// <param name="login">Логин аккаунта пользователя</param>
	private void HandleEntityBaseCreatedAndModifiedMetadata(string login)
	{
		var changeSet = ChangeTracker.Entries<EntityBase>();

		if (changeSet?.Any() == true)
		{
			foreach (var entry in changeSet.Where(c => c.State == EntityState.Added))
			{
				entry.Entity.CreatedOn = _dateTimeProvider.UtcNow;
				entry.Entity.CreatedBy = login;
				entry.Entity.ModifiedOn = _dateTimeProvider.UtcNow;
				entry.Entity.ModifiedBy = login;
			}

			foreach (var entry in changeSet.Where(c => c.State == EntityState.Modified))
			{
				entry.Entity.ModifiedOn = _dateTimeProvider.UtcNow;
				entry.Entity.ModifiedBy = login;
			}
		}
	}

	/// <summary>
	/// Обработать мягкое удаления сущностей
	/// </summary>
	/// <param name="login">Логин аккаунта пользователя</param>
	private void HandleSoftDelete(string login)
	{
		var deleteChangeSet = ChangeTracker.Entries<ISoftDeletable>();

		if (deleteChangeSet?.Any() == true)
		{
			foreach (var entry in deleteChangeSet.Where(c => c.State == EntityState.Deleted))
			{
				entry.Entity.RevokedOn = _dateTimeProvider.UtcNow;
				entry.Entity.RevokedBy = login;
				entry.State = EntityState.Modified;
			}
		}
	}
}
