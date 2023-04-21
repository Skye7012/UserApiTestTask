using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Сервис пользователя
/// </summary>
public interface IUserService
{
	/// <summary>
	/// Получить логин по клейму в токене
	/// </summary>
	/// <returns>Логин</returns>
	string GetLogin();

	/// <summary>
	/// Создать токен авторизации
	/// </summary>
	/// <param name="user">Пользователь, для которого создается токен</param>
	/// <returns>Токен авторизации</returns>
	string CreateToken(User user);

	/// <summary>
	/// Создать хэш пароля
	/// </summary>
	/// <param name="password">Пароль</param>
	/// <param name="passwordHash">Итоговый захэшированный пароль</param>
	/// <param name="passwordSalt">Соль захэшированного пароля</param>
	void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

	/// <summary>
	/// Проверить валидность пароля
	/// </summary>
	/// <param name="password">Пароль</param>
	/// <param name="passwordHash">Хэш пароля</param>
	/// <param name="passwordSalt">Соль пароля</param>
	/// <returns>Валиден ли пароль</returns>
	bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);

	/// <summary>
	/// Аутентифицировался ли пользователь
	/// </summary>
	/// <returns>Аутентифицировался ли пользователь</returns>
	bool IsAuthenticated();

	/// <summary>
	/// Является ли авторизованный пользователь администратором
	/// </summary>
	/// <returns>Является ли авторизованный пользователь администратором</returns>
	bool IsAdmin();

	/// <summary>
	/// Проверить, что переданный пользователь соответствует аутентифицированному,
	///  и что переданный пользователь активен<br/>
	/// Либо авторизованный пользователь является администратором
	/// </summary>
	/// <param name="user">Пользователь</param>
	void CheckUserPermissionRule(User user);
}
