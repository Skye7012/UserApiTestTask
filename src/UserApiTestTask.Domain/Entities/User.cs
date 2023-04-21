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
	/// Поля для <see cref="Gender"/>
	/// </summary>
	private Gender _gender;

	/// <summary>
	/// Поля для <see cref="Login"/>
	/// </summary>
	private string _login = default!;

	/// <summary>
	/// Поля для <see cref="Name"/>
	/// </summary>
	private string _name = default!;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="login">Логин</param>
	/// <param name="passwordHash">Хэш пароля</param>
	/// <param name="passwordSalt">Соль пароля</param>
	/// <param name="name">Имя</param>
	/// <param name="gender">Пол</param>
	/// <param name="birthDay">Дата рождения</param>
	/// <param name="isAdmin">Является ли пользователь администратором</param>
	public User(
		string login,
		byte[] passwordHash,
		byte[] passwordSalt,
		string name,
		Gender gender,
		DateTime? birthDay,
		bool isAdmin)
	{
		Login = login;
		PasswordHash = passwordHash;
		PasswordSalt = passwordSalt;
		Name = name;
		Gender = gender;
		BirthDay = birthDay;
		IsAdmin = isAdmin;
	}

	/// <summary>
	/// Конструктор
	/// </summary>
	public User() { }

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
	public Gender Gender
	{
		get => _gender;
		set
		{
			if (!Enum.IsDefined(typeof(Gender), value))
				throw new ValidationProblem($"{value} не входит в перечисление {nameof(Gender)}");

			_gender = value;
		}
	}

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
}
