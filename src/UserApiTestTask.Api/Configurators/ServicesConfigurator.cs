using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserApiTestTask.Api.Filters;
using UserApiTestTask.Api.Services;
using UserApiTestTask.Application;
using UserApiTestTask.Application.Common.Interfaces;
using UserApiTestTask.Infrastructure;

namespace UserApiTestTask.Api.Configurators;

/// <summary>
/// Конфигуратор сервисов
/// </summary>
public static class ServicesConfigurator
{
	/// <summary>
	/// Сконфигурировать сервисы
	/// </summary>
	/// <param name="builder">Билдер приложения</param>
	public static void ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddControllers(opt =>
			{
				opt.Filters.Add<VoidAndTaskTo204NoContentFilter>();
				opt.SuppressAsyncSuffixInActionNames = false;
			})
			.AddJsonOptions(opt =>
			{
				opt.JsonSerializerOptions.Converters
					.Add(new JsonStringEnumConverter(allowIntegerValues: false));
			});

		builder.Services
			.AddEndpointsApiExplorer()
			.AddHttpContextAccessor()
			.AddCors();

		builder.Services
			.AddInfrastructureServices(builder.Configuration)
			.AddApplicationServices();

		builder.Services
			.AddSwagger();

		builder.Services
			.AddSingleton<IDateTimeProvider, DateTimeProvider>();
	}

	/// <summary>
	/// Сконфигурировать Swagger
	/// </summary>
	/// <param name="services">Сервисы</param>
	private static IServiceCollection AddSwagger(this IServiceCollection services)
		=> services
			.AddSwaggerGen(options =>
			{
				options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
				{
					Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
					In = ParameterLocation.Header,
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey
				});

				options.OperationFilter<SecurityRequirementsOperationFilter>();
				options.SupportNonNullableReferenceTypes();

				IncludeProjectXmlComments(options, typeof(Program).Assembly);
				IncludeProjectXmlComments(options, typeof(ApplicationServicesConfigurator).Assembly);
				IncludeProjectXmlComments(options, typeof(Domain.Entities.Common.EntityBase).Assembly);
				IncludeProjectXmlComments(options, typeof(Contracts.Requests.Common.BaseGetRequest).Assembly);
			});

	/// <summary>
	/// Включить XML комментарии из проекта
	/// </summary>
	/// <param name="options">Опции настройки Swagger</param>
	/// <param name="assembly">Сборка проекта</param>
	private static void IncludeProjectXmlComments(SwaggerGenOptions options, Assembly assembly)
	{
		var xmlFilename = $"{assembly.GetName().Name}.xml";
		options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
	}
}
