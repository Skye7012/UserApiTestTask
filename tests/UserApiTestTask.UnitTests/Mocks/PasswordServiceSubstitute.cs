using NSubstitute;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Infrastructure.Services;
using UserApiTestTask.UnitTests.Common.Interfaces;

namespace UserApiTestTask.UnitTests.Mocks;

/// <summary>
/// Сервис паролей для тестов
/// </summary>
public class PasswordServiceSubstitute : IPasswordService, ISubstitute<IPasswordService>
{
	private readonly PasswordService _service;

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="adminUser">пользователь-администратор</param>
	public PasswordServiceSubstitute()
	{
		_service = new PasswordService();
	}

	/// <inheritdoc/>
	public IPasswordService Create()
		=> Substitute.ForPartsOf<PasswordServiceSubstitute>();

	/// <inheritdoc/>
	public virtual void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		=> _service.CreatePasswordHash(password, out passwordHash, out passwordSalt);

	/// <inheritdoc/>
	public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		=> _service.VerifyPasswordHash(password, passwordHash, passwordSalt);
}
