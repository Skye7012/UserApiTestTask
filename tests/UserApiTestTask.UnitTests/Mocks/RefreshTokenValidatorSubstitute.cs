using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.UnitTests.Common.Interfaces;

namespace UserApiTestTask.UnitTests.Mocks;

/// <summary>
/// Валидатор Refresh токенов для тестов
/// </summary>
public class RefreshTokenValidatorSubstitute : IRefreshTokenValidator, ISubstitute<IRefreshTokenValidator>
{
	private readonly UserAccount _userAccount;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="userAccount">Аккаунт пользователя</param>
	public RefreshTokenValidatorSubstitute(UserAccount userAccount)
		=> _userAccount = userAccount;

	/// <inheritdoc/>
	public IRefreshTokenValidator Create()
		=> Substitute.ForPartsOf<RefreshTokenValidatorSubstitute>(_userAccount);

	/// <inheritdoc/>
	public virtual async Task<UserAccount> ValidateAndReceiveUserAsync(
		IApplicationDbContext context,
		string refreshToken,
		CancellationToken cancellationToken = default)
			=> await context.UserAccounts.FirstAsync(x => x.Id == _userAccount.Id, cancellationToken);
}
