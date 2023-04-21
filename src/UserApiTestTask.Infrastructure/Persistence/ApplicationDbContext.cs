using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Entities.Common;

namespace UserApiTestTask.Infrastructure.Persistence;

/// <inheritdoc/>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
	private readonly IUserService _userService;
	private readonly IDateTimeProvider _dateTimeProvider;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="options">Опции контекста</param>
	/// <param name="userService">Сервис пользователя</param>
	/// <param name="dateTimeProvider">Провайдер даты и времени</param>
	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options,
		IUserService userService,
		IDateTimeProvider dateTimeProvider)
		: base(options)
	{
		_userService = userService;
		_dateTimeProvider = dateTimeProvider;
	}

	/// <inheritdoc/>
	public DbContext Instance => this;

	/// <summary>
	/// Пользователи
	/// </summary>
	public DbSet<User> Users { get; private set; } = default!;

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

	/// <inheritdoc/>
	public int SaveChanges(bool withSoftDelete = true, bool acceptAllChangesOnSuccess = true)
	{
		HandleSaveChangesLogic(withSoftDelete);

		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	/// <inheritdoc/>
	public async Task<int> SaveChangesAsync(
		bool withSoftDelete = true,
		bool acceptAllChangesOnSuccess = true,
		CancellationToken cancellationToken = default)
	{
		HandleSaveChangesLogic(withSoftDelete);

		return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	/// <summary>
	/// Сохранить изменения
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Количество записей состояния, записанных в базу данных</returns>
	public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		HandleSaveChangesLogic(true);

		return await base.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Обработать логику сохранения изменений
	/// </summary>
	/// <param name="withSoftDelete">Использовать мягкое удаление</param>
	private void HandleSaveChangesLogic(bool withSoftDelete)
	{
		if (!_userService.IsAuthenticated())
			return;

		var changeSet = ChangeTracker.Entries<EntityBase>();

		if (changeSet?.Any() == true)
		{
			foreach (var entry in changeSet.Where(c => c.State == EntityState.Added))
			{
				entry.Entity.CreatedOn = _dateTimeProvider.UtcNow;
				entry.Entity.CreatedBy = _userService.GetLogin();
				entry.Entity.ModifiedOn = _dateTimeProvider.UtcNow;
				entry.Entity.ModifiedBy = _userService.GetLogin();
			}

			foreach (var entry in changeSet.Where(c => c.State == EntityState.Modified))
			{
				entry.Entity.ModifiedOn = _dateTimeProvider.UtcNow;
				entry.Entity.ModifiedBy = _userService.GetLogin();
			}
		}

		if (!withSoftDelete)
			return;

		var deleteChangeSet = ChangeTracker.Entries<ISoftDeletable>();

		if (deleteChangeSet?.Any() == true)
		{
			foreach (var entry in deleteChangeSet.Where(c => c.State == EntityState.Deleted))
			{
				entry.Entity.RevokedOn = _dateTimeProvider.UtcNow;
				entry.Entity.RevokedBy = _userService.GetLogin();
				entry.State = EntityState.Modified;
			}
		}
	}
}
