using FluentAssertions;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Domain.Exceptions;
using Xunit;

namespace UserApiTestTask.UnitTests.Entities;

/// <summary>
/// Тесты для <see cref="User"/>
/// </summary>
public class UserTests : UnitTestBase
{
	/// <summary>
	/// Должен выкинуть ошибку, когда указано пустое имя
	/// </summary>
	[Fact]
	public void Name_ShouldThrow_WhenEmptyName()
	{
		var context = CreateInMemoryContextAsync();

		var act = () => new User(
				" ",
				Gender.Female,
				null,
				true,
				null!);

		act.Should()
			.Throw<ValidationProblem>()
			.WithMessage($"Поле {nameof(User.Name)} не может быть пустым");
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда указано невалидное имя
	/// </summary>
	[Fact]
	public void Тфьу_ShouldThrow_WhenInvalidТфьу()
	{
		var context = CreateInMemoryContextAsync();

		var act = () => new User(
				"Невалидное имя",
				Gender.Female,
				null,
				true,
				null!);

		act.Should()
			.Throw<ValidationProblem>()
			.WithMessage($"Для имени запрещены все символы кроме латинских и русских букв");
	}
}
