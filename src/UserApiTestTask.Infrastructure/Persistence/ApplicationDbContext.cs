using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Exceptions;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Entities.Common;
using UserApiTestTask.Domain.Exceptions;
using UserApiTestTask.Infrastructure.Extensions;
using UserApiTestTask.Infrastructure.InitExecutors;

namespace UserApiTestTask.Infrastructure.Persistence;

/// <summary>
/// Контекст БД данного приложения
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
	private readonly IAuthorizationService _authorizationService;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IUserAccountCacheRepository _userAccountCache;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="options">Опции контекста</param>
	/// <param name="authorizationService">Сервис авторизации</param>
	/// <param name="dateTimeProvider">Провайдер даты и времени</param>
	/// <param name="userAccountCache">Репозиторий кэширования аккаунта пользователя</param>
	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options,
		IAuthorizationService authorizationService,
		IDateTimeProvider dateTimeProvider,
		IUserAccountCacheRepository userAccountCache)
		: base(options)
	{
		_authorizationService = authorizationService;
		_dateTimeProvider = dateTimeProvider;
		_userAccountCache = userAccountCache;
	}

	/// <inheritdoc/>
	public DbContext Instance => this;

	/// <inheritdoc/>
	public DbSet<User> Users { get; private set; } = default!;

	/// <inheritdoc/>
	public DbSet<UserAccount> UserAccounts { get; private set; } = default!;

	/// <inheritdoc/>
	public DbSet<RefreshToken> RefreshTokens { get; private set; } = default!;

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
				entityType.AddISoftDeletableFields();
		}
	}

	/// <inheritdoc/>
	public override int SaveChanges()
		=> throw new NotImplementedException("Используй асинхронные методы");

	/// <inheritdoc/>
	public override int SaveChanges(bool acceptAllChangesOnSuccess)
		=> throw new NotImplementedException("Используй асинхронные методы");

	/// <inheritdoc/>
	public async Task<int> SaveChangesAsync(
		bool withSoftDelete = true,
		bool acceptAllChangesOnSuccess = true,
		CancellationToken cancellationToken = default)
	{
		await HandleSaveChangesLogicAsync(withSoftDelete, cancellationToken);

		return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<int> SaveChangesAsync()
		=> await SaveChangesAsync((CancellationToken)default);

	/// <inheritdoc/>
	public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await HandleSaveChangesLogicAsync(true, cancellationToken);

		return await base.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc/>
	private async Task HandleSaveChangesLogicAsync(bool withSoftDelete, CancellationToken cancellationToken)
	{
		var willLoginBeNeeded = ChangeTracker.Entries<EntityBase>()?.Any() == true
			|| ChangeTracker.Entries<ISoftDeletable>()?.Any() == true;

		if (!willLoginBeNeeded)
			return;

		var login = _authorizationService.IsAuthenticated()
			? await GetCachedLoginAsync(cancellationToken)
			: GetLoginWhenUnauthorized();

		HandleEntityBaseCreatedAndModifiedMetadata(login);

		if (!withSoftDelete)
			return;

		HandleSoftDelete(login);
	}

	/// <summary>
	/// Получить логин в случае, когда пользователь не авторизован
	/// </summary>
	/// <returns>Логин</returns>
	private string GetLoginWhenUnauthorized()
	{
		var refreshTokens = ChangeTracker.Entries<RefreshToken>();

		if (!refreshTokens.Any())
			return DbInitExecutor.AdminLogin;

		var login = refreshTokens.First().Entity.UserAccount!.Login;

		var isLoginSame = refreshTokens
			.Where(x => x.State != EntityState.Deleted)
			.All(x => x.Entity.UserAccount!.Login == login);

		return !isLoginSame
			? throw new ApplicationProblem("Не удается получить валидный логин")
			: login;
	}

	/// <summary>
	/// Получить закешированный логин
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Закешированный логин</returns>
	private async Task<string> GetCachedLoginAsync(CancellationToken cancellationToken)
	{
		var login = await _userAccountCache.GetLoginAsync(
			_authorizationService.GetUserAccountId(),
			cancellationToken);

		if (login == null)
		{
			login = (await UserAccounts
				.FirstOrDefaultAsync(x => _authorizationService.GetUserAccountId() == x.Id, cancellationToken))
					?.Login
				?? throw new EntityNotFoundProblem<UserAccount>(_authorizationService.GetUserAccountId());

			await _userAccountCache.SetLoginAsync(
				_authorizationService.GetUserAccountId(),
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
