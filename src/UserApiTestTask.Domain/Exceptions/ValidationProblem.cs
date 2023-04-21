using System.Net;

namespace UserApiTestTask.Domain.Exceptions;

/// <summary>
/// Ошибка валидации
/// </summary>
public class ValidationProblem : ApplicationProblem
{

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="message">Сообщение</param>
	public ValidationProblem(string message) : base(message)
	{ }

	/// <inheritdoc/>
	public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}
