using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UserApiTestTask.IntegrationTests.MediaTypeFormatters;

/// <summary>
/// HTTP ответ с сериализованным результатом
/// </summary>
/// <typeparam name="TResult">Тип сериализованного результата</typeparam>
public class ProblemMediaTypeFormatter : MediaTypeFormatter
{
	public ProblemMediaTypeFormatter()
	{
		SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/problem+json"));
	}

	public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		=> ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);

	public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
		=> (await JsonSerializer.DeserializeAsync<ProblemDetails>(
			readStream,
			cancellationToken: cancellationToken))!;

	public override bool CanReadType(Type type)
	{
		return type == typeof(ProblemDetails);
	}

	public override bool CanWriteType(Type type)
	{
		return false;
	}
}
