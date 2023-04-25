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
	/// Проверить, что переданный пользователь соответствует аутентифицированному,
	///  и что переданный пользователь активен<br/>
	/// Либо аутентифицированный пользователь является администратором
	/// </summary>
	/// <param name="userAccount">Аккаунт пользователя</param>
	void CheckUserPermissionRule(UserAccount userAccount);
}
