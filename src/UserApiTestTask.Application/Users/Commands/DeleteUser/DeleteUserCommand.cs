using MediatR;

namespace UserApiTestTask.Application.Users.Commands.DeleteUser;

/// <summary>
/// Команда на удаление пользователя
/// </summary>
public class DeleteUserCommand : IRequest
{
	/// <summary>
	/// Логин
	/// </summary>
	public string Login { get; set; } = default!;

	/// <summary>
	/// Использовать мягкое удаление
	/// </summary>
	public bool WithSoftDelete { get; set; }
}
