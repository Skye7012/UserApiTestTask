using UserApiTestTask.Contracts.Common.Enums;

namespace UserApiTestTask.Contracts.Requests.Users.PutUser;

/// <summary>
/// Запрос на обновление данных о пользователе
/// </summary>
public class PutUserRequest
{
	/// <summary>
	/// Имя
	/// </summary>
	public string Name { get; set; } = default!;

	/// <summary>
	/// Пол
	/// </summary>
	public Gender Gender { get; set; }

	/// <summary>
	/// Дата рождения
	/// </summary>
	public DateTime? BirthDay { get; set; }
}
