using System.Net;

namespace UserApiTestTask.Domain.Exceptions;

/// <summary>
/// Базовая ошибка приложения
/// </summary>
public class ApplicationProblem : Exception
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="message">Сообщение</param>
	public ApplicationProblem(string message) : base(message)
	{ }

	/// <summary>
	/// Код состояния
	/// </summary>
	public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}
