using Microsoft.Extensions.Caching.Distributed;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.Application.Common.Static;

namespace UserApiTestTask.Infrastructure.Services.CacheRepositories;

/// <summary>
/// Репозиторий кэширования аккаунта пользователя
/// </summary>
public class UserAccountCacheRepository : IUserAccountCacheRepository
{
	private readonly IDistributedCache _cache;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="cache">Сервис кэширования</param>
	public UserAccountCacheRepository(IDistributedCache cache)
		=> _cache = cache;

	/// <inheritdoc/>
	public async Task<string?> GetLoginAsync(Guid userAccountId, CancellationToken cancellationToken)
		=> await _cache.GetStringAsync(
			RedisKeys.GetUserAccountLoginKey(userAccountId),
			cancellationToken);

	/// <inheritdoc/>
	public async Task SetLoginAsync(Guid userAccountId, string login, CancellationToken cancellationToken)
		=> await _cache.SetStringAsync(
				RedisKeys.GetUserAccountLoginKey(userAccountId),
				login,
				cancellationToken);
}
