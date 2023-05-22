using System.Text.RegularExpressions;
using UserApiTestTask.Domain.Entities.Common;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Domain.Entities;

/// <summary>
/// Аккаунт пользователя
/// </summary>
public class UserAccount : EntityBase, ISoftDeletable
{
	/// <summary>
	/// Наименование backing field <see cref="_refreshTokens"/>
	/// </summary>
	public static readonly string RefreshTokensFieldName = nameof(_refreshTokens);

	/// <summary>
	/// Поле для <see cref="Login"/>
	/// </summary>
	private string _login = default!;

	/// <summary>
	/// Поле для <see cref="RefreshTokens"/>
	/// </summary>
	private readonly List<RefreshToken>? _refreshTokens;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	/// <param name="passwordHash">Хэш пароля</param>
	/// <param name="passwordSalt">Соль пароля</param>
	public UserAccount(
		string login,
		byte[] passwordHash,
		byte[] passwordSalt)
	{
		Login = login;
		PasswordHash = passwordHash;
		PasswordSalt = passwordSalt;

		_refreshTokens = new List<RefreshToken>();
	}

	/// <summary>
	/// Конструктор
	/// </summary>
	public UserAccount() { }

	/// <summary>
	/// Логин
	/// </summary>
	public string Login
	{
		get => _login;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ValidationProblem($"Поле {nameof(Login)} не может быть пустым");

			if (!Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
				throw new ValidationProblem("Для логина запрещены все символы кроме латинских букв и цифр");

			_login = value;
		}
	}

	/// <summary>
	/// Хэш пароля
	/// </summary>
	public byte[] PasswordHash { get; set; } = default!;

	/// <summary>
	/// Соль пароля
	/// </summary>
	public byte[] PasswordSalt { get; set; } = default!;

	/// <inheritdoc/>
	public DateTime? RevokedOn { get; set; }

	/// <inheritdoc/>
	public string? RevokedBy { get; set; }

	/// <summary>
	/// Деактивировать все refresh токены
	/// </summary>
	public void RevokeAllRefreshTokens()
	{
		if (_refreshTokens == null)
			throw new NotIncludedProblem(nameof(_refreshTokens));

		_refreshTokens.Clear();
	}

	/// <summary>
	/// Добавить refresh токен
	/// </summary>
	public void AddRefreshToken(RefreshToken refreshToken)
	{
		if (_refreshTokens == null)
			throw new NotIncludedProblem(nameof(_refreshTokens));


		var activeTokens = _refreshTokens.Where(r => r.RevokedOn == null);

		if (activeTokens.Count() >= 5)
			_refreshTokens.Remove(
				activeTokens.First(x => x.CreatedOn == activeTokens.Max(x => x.CreatedOn)));

		_refreshTokens.Add(refreshToken);
	}

	#region navigation Properties

	/// <summary>
	/// Пользователь
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Refresh токены
	/// </summary>
	public IReadOnlyList<RefreshToken>? RefreshTokens => _refreshTokens;

	#endregion
}
