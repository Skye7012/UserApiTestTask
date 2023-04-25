namespace UserApiTestTask.Application.Common.Static;

/// <summary>
/// Методы для формирования ключей redis
/// </summary>
public class RedisKeys
{
	/// <summary>
	/// Получить ключ для закэшированного логина аккаунта пользователя
	/// </summary>
	/// <param name="id">Идентификатор аккаунта пользователя</param>
	/// <returns>Ключ для закэшированного логина аккаунта пользователя</returns>
	public static string GetUserAccountLoginKey(Guid id)
		=> $"userAccount:{id}:login";
}
