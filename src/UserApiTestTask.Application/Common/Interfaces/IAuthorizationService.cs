using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Сервис авторизации
/// </summary>
public interface IAuthorizationService
{
	/// <summary>
	/// Аутентифицировался ли пользователь
	/// </summary>
	/// <returns>Аутентифицировался ли пользователь</returns>
	bool IsAuthenticated();

	/// <summary>
	/// Получить идентификатор пользователя по клейму в токене
	/// </summary>
	/// <returns>Идентификатор пользователя</returns>
	public Guid GetUserId();

	/// <summary>
	/// Получить идентификатор аккаунта пользователя по клейму в токене
	/// </summary>
	/// <returns>Идентификатор аккаунта пользователя</returns>
	public Guid GetUserAccountId();

	/// <summary>
	/// Является ли авторизованный пользователь администратором
	/// </summary>
	/// <returns>Является ли авторизованный пользователь администратором</returns>
	bool IsAdmin();

	/// <summary>
	/// Проверить, что авторизованный пользователь является активным администратором
	/// </summary>
	void CheckIsAdmin();

	/// <summary>
	/// Проверить, что переданный пользователь соответствует аутентифицированному, 
	/// либо является администратором<br/>
	/// Пользователь также должен быть активным
	/// </summary>
	/// <param name="userAccount">Аккаунт пользователя</param>
	void CheckIsUserAdminOrUserItself(UserAccount userAccount);
}
