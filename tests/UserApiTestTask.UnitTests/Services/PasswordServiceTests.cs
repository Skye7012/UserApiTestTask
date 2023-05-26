using FluentAssertions;
using UserApiTestTask.Infrastructure.Services;
using Xunit;

namespace UserApiTestTask.UnitTests.Services;

/// <summary>
/// Тесты для <see cref="PasswordService"/>
/// </summary>
public class PasswordServiceTests : UnitTestBase
{
	private readonly PasswordService _sut;

	/// <inheritdoc/>
	public PasswordServiceTests()
		=> _sut = new PasswordService();

	/// <summary>
	/// Созданный пароль должен быть валидным
	/// </summary>
	[Fact]
	public void CreatePasswordHash_ShoudCreateValidHashAndSalt()
	{
		var password = "jKw7Oae8Tb0f3sYp";

		_sut.CreatePasswordHash(
			password,
			out var hash,
			out var salt);

		_sut.VerifyPasswordHash(password, hash, salt)
			.Should().BeTrue();

		_sut.VerifyPasswordHash(password, salt, hash)
			.Should().BeFalse();
	}
}
