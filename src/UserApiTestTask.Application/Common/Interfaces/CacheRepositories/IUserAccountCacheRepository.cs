namespace UserApiTestTask.Application.Common.Interfaces.CacheRepositories;

/// <summary>
/// Репозиторий кэширования аккаунта пользователя
/// </summary>
public interface IUserAccountCacheRepository
{
	/// <summary>
	/// Получить логин по идентификатору аккаунта пользователя
	/// </summary>
	/// <param name="userAccountId">Идентификатор аккаунта пользователя</param>
	/// <param name="cancellationToken">Логин</param>
	Task<string?> GetLoginAsync(
		Guid userAccountId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Присвоить логин аккаунту пользователя по его идентификатору
	/// </summary>
	/// <param name="userAccountId">Идентификатор аккаунта пользователя</param>
	/// <param name="login">Логин</param>
	/// <param name="cancellationToken">Токен отмены</param>
	Task SetLoginAsync(
		Guid userAccountId,
		string login,
		CancellationToken cancellationToken);
}
