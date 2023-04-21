using UserApiTestTask.Contracts.Common.Enums;

namespace UserApiTestTask.Contracts.Requests.Users.GetUser;

/// <summary>
/// Ответ на запрос получения данных о пользователе
/// </summary>
public class GetUserResponse
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

	/// <summary>
	/// Является ли пользователь активным
	/// </summary>
	public bool IsActive { get; set; }
}
