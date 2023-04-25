using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Common.Extensions;

/// <summary>
/// Расширения для <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Аутентифицировался ли пользователь
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <param name="configuration">Конфигурация приложения</param>
	/// <param name="key">Путь к конфигурации</param>
	/// <returns>Аутентифицировался ли пользователь</returns>
	public static TOptions ConfigureAndGet<TOptions>(
		this IServiceCollection source,
		IConfiguration configuration,
		string key)
		where TOptions : class
	{
		var configSection = configuration.GetSection(key);

		var config = configSection.Get<TOptions>()
			?? throw new ApplicationProblem($"Не удается загрузить конфигурацию для {typeof(TOptions).Name}");

		source.Configure<TOptions>(configSection);

		return config;
	}
}
