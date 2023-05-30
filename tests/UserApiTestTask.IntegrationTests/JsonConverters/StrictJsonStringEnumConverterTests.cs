using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using UserApiTestTask.Contracts.Common.Enums;
using UserApiTestTask.Contracts.Requests.Users.PutUser;
using UserApiTestTask.Infrastructure.Persistence;
using UserApiTestTask.IntegrationTests.Extensions;
using Xunit;

namespace UserApiTestTask.IntegrationTests.JsonConverters;

/// <summary>
/// Тесты для <see cref="StrictJsonStringEnumConverter"/>
/// </summary>
public class StrictJsonStringEnumConverterTests : IntegrationTestsBase
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="factory">Фабрика приложения</param>
	public StrictJsonStringEnumConverterTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : base(factory)
	{
	}

	/// <summary>
	/// Должен выкинуть ошибку, когда значение не входит в перечисление
	/// </summary>
	[Fact]
	public async Task Read_ShouldThrow_WhenValueDoesntDefinedInEnum()
	{
		Authenticate();

		var request = new PutUserRequest
		{
			Name = "NewName",
			Gender = (Gender)999,
			BirthDay = DateTimeProvider.UtcNow.AddDays(8),
		};

		var response = await Client.PutUserAsync(Seeder.AdminUserAccount.Login, request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		var problemDetails = await response.Content.ReadAsAsync<ProblemDetails>();

		problemDetails.Detail.Should().NotBeNullOrWhiteSpace();
		problemDetails.Detail.Should().Be("Значение '999' не входит в перечисление Gender");
	}
}
