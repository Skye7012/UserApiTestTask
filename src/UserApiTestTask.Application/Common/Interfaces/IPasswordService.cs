namespace UserApiTestTask.Application.Common.Interfaces;

/// <summary>
/// Сервис паролей
/// </summary>
public interface IPasswordService
{
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
}
