using Microsoft.EntityFrameworkCore;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Интерфейс контекста БД
/// </summary>
public interface IDbContext : IDisposable
{
	/// <summary>
	/// Экземпляр текущего контекста БД
	/// </summary>
	DbContext Instance { get; }
}
