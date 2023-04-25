using System.Net;

namespace UserApiTestTask.Domain.Exceptions;

/// <summary>
/// Навигационное свойство не было загружено
/// </summary>
public class NotIncludedProblem : ApplicationProblem
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="navifationProperty">Навигационные свойство</param>
	public NotIncludedProblem(string navifationProperty)
		: base($"Навигационное свойство {navifationProperty} не было загружено")
	{ }

	/// <inheritdoc/>
	public override HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
}
