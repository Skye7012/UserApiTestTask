using UserApiTestTask.Domain.Entities.Common;

namespace UserApiTestTask.Domain.Entities;

/// <summary>
/// Refresh токен
/// </summary>
public class RefreshToken : EntityBase, ISoftDeletable
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="token">Токен</param>
	/// <param name="userAccount">Аккаунт пользователя</param>
	public RefreshToken(
		string token,
		UserAccount userAccount)
	{
		Token = token;
		UserAccount = userAccount;
	}

	/// <summary>
	/// Конструктор
	/// </summary>
	public RefreshToken() { }

	/// <summary>
	/// Токен
	/// </summary>
	public string Token { get; set; } = default!;

	/// <inheritdoc/>
	public DateTime? RevokedOn { get; set; }

	/// <inheritdoc/>
	public string? RevokedBy { get; set; }

	/// <summary>
	/// Идентификатор Аккаунта пользователя
	/// </summary>
	public Guid UserAccountId { get; set; }

	#region navigation Properties

	/// <summary>
	/// Аккаунт пользователя
	/// </summary>
	public UserAccount? UserAccount { get; set; }

	#endregion
}
