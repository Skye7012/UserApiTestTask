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
	public ForbiddenProblem()
		: base("У пользователя нет прав на выполнение данной операции")
	{ }

	/// <inheritdoc/>
	public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
}
