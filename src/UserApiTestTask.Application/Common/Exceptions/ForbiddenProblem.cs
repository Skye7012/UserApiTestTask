using System.Net;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Common.Exceptions;

/// <summary>
/// У пользователя нет прав на выполнение данной операции 
/// </summary>
public class ForbiddenProblem : ApplicationProblem
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="message">Сообщение</param>
	public ForbiddenProblem(string? message = null)
		: base(message ?? "У пользователя нет прав на выполнение данного действия")
	{ }

	/// <inheritdoc/>
	public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
}
