using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserApiTestTask.IntegrationTests.MediaTypeFormatters;

namespace UserApiTestTask.IntegrationTests.Common;

/// <summary>
/// HTTP ответ с сериализованным результатом
/// </summary>
/// <typeparam name="TResult">Тип сериализованного результата</typeparam>
public class HttpResponseMessageWithResult<TResult>
	where TResult : class
{
	/// <summary>
	/// Форматтеры
	/// </summary>
	private static readonly List<MediaTypeFormatter> Formatters = new()
	{
		new JsonMediaTypeFormatter(),
		new ProblemMediaTypeFormatter(),
	};

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="httpResponseMessage">HTTP ответ</param>
	/// <param name="result">Сериализованный результат</param>
	/// <param name="problemDetails">Ошибка</param>
	public HttpResponseMessageWithResult(
		HttpResponseMessage httpResponseMessage,
		TResult? result,
		ProblemDetails? problemDetails)
	{
		Message = httpResponseMessage;
		Result = result;
		Error = problemDetails;
	}

	/// <summary>
	/// Создать экземпляр асинхронно
	/// </summary>
	/// <param name="response">HTTP ответ</param>
	/// <returns>HTTP ответ с сериализованным результатом</returns>
	public static async Task<HttpResponseMessageWithResult<TResult>>
		CreateAsync(HttpResponseMessage response)
	{
		return !response.IsSuccessStatusCode
			? new HttpResponseMessageWithResult<TResult>(
				response,
				null,
				await response.Content.ReadAsAsync<ProblemDetails>(Formatters))
			: new HttpResponseMessageWithResult<TResult>(
				response,
				await response.Content.ReadAsAsync<TResult>(),
				null);
	}

	/// <summary>
	/// HTTP ответ
	/// </summary>
	public HttpResponseMessage Message { get; }

	/// <summary>
	/// Сериализованный результат
	/// </summary>
	public TResult? Result { get; }

	/// <summary>
	/// Ошибка
	/// </summary>
	public ProblemDetails? Error { get; }

	/// <summary>
	/// Прошел ли запрос успешно
	/// </summary>
	public bool IsSuccessfulResult => Message.IsSuccessStatusCode;
}
