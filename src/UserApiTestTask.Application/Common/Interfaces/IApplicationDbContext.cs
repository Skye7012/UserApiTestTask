using Microsoft.EntityFrameworkCore;
using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Интерфейс контекста БД данного приложения
/// </summary>
public interface IApplicationDbContext : IDbContext
{
	/// <summary>
	/// Пользователи
	/// </summary>
	DbSet<User> Users { get; }

	/// <summary>
	/// Аккаунты пользователей
	/// </summary>
	DbSet<UserAccount> UserAccounts { get; }

	/// <summary>
	/// Refresh токены
	/// </summary>
	DbSet<RefreshToken> RefreshTokens { get; }

	/// <summary>
	/// Сохранить изменения
	/// </summary>
	/// <param name="withSoftDelete">Использовать мягкое удаление</param>
	/// <param name="acceptAllChangesOnSuccess">Указывает, вызывается ли AcceptAllChanges() после успешной отправки изменений в базу данных</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Количество записей состояния, записанных в базу данных</returns>
	Task<int> SaveChangesAsync(
		bool withSoftDelete = true,
		bool acceptAllChangesOnSuccess = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Сохранить изменения
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>Количество записей состояния, записанных в базу данных</returns>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Сохранить изменения
	/// </summary>
	/// <returns>Количество записей состояния, записанных в базу данных</returns>
	Task<int> SaveChangesAsync();
}
