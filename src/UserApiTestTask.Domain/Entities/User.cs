using System.Text.RegularExpressions;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities.Common;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Domain.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : EntityBase, ISoftDeletable
{
	/// <summary>
	/// Поля для <see cref="Name"/>
	/// </summary>
	private string _name = default!;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="name">Имя</param>
	/// <param name="gender">Пол</param>
	/// <param name="birthDay">Дата рождения</param>
	/// <param name="isAdmin">Является ли пользователь администратором</param>
	/// <param name="userAccount">Аккаунт пользователя</param>
	public User(
		string name,
		Gender gender,
		DateTime? birthDay,
		bool isAdmin,
		UserAccount userAccount)
	{
		Name = name;
		Gender = gender;
		BirthDay = birthDay;
		IsAdmin = isAdmin;
		UserAccount = userAccount;
	}

	/// <summary>
	/// Конструктор
	/// </summary>
	public User() { }

	/// <summary>
	/// Имя
	/// </summary>
	public string Name
	{
		get => _name;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ValidationProblem($"Поле {nameof(Name)} не может быть пустым");

			if (!Regex.IsMatch(value, @"^[a-zA-Zа-яА-я]+$"))
				throw new ValidationProblem("Для имени запрещены все символы кроме латинских и русских букв");

			_name = value;
		}
	}

	/// <summary>
	/// Пол
	/// </summary>
	public Gender Gender { get; set; }

	/// <summary>
	/// Дата рождения
	/// </summary>
	public DateTime? BirthDay { get; set; }

	/// <summary>
	/// Является ли пользователь администратором
	/// </summary>
	public bool IsAdmin { get; set; }

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
