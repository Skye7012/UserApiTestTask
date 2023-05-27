using UserApiTestTask.Domain.Entities;

namespace UserApiTestTask.IntegrationTests.Mocks;

/// <summary>
/// Тестовый пользователь с не захэшированным паролем
/// </summary>
public class TestUserAccount : UserAccount
{
	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; } = default!;
}
