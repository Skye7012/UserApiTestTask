using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces.CacheRepositories;
using UserApiTestTask.UnitTests.Common.Interfaces;

namespace UserApiTestTask.UnitTests.Mocks.CacheRepositories;

/// <summary>
/// Репозиторий закэшированных идентификаторов ссылок загрузки файла
/// </summary>
public class UserAccountCacheRepositorySubstitute : IUserAccountCacheRepository, ISubstitute<IUserAccountCacheRepository>
{
	private readonly Dictionary<Guid, string> _loginByUserAccountIds = new();

	/// <summary>
	/// Конструктор
	/// </summary>
	public UserAccountCacheRepositorySubstitute()
	{ }

	/// <inheritdoc/>
	public IUserAccountCacheRepository Create()
		=> Substitute.ForPartsOf<UserAccountCacheRepositorySubstitute>();

	/// <inheritdoc/>
	public Task<string?> GetLoginAsync(Guid userAccountId, CancellationToken cancellationToken)
	{
		var res = !_loginByUserAccountIds.ContainsKey(userAccountId)
			? null
			: _loginByUserAccountIds[userAccountId];

		return Task.FromResult(res);
	}

	/// <inheritdoc/>
	public Task SetLoginAsync(Guid userAccountId, string login, CancellationToken cancellationToken)
	{
		_loginByUserAccountIds[userAccountId] = login;

		return Task.CompletedTask;
	}
}
