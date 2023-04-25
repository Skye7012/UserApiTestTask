using System.Net;
using UserApiTestTask.Domain.Entities.Common;
using UserApiTestTask.Domain.Exceptions;

namespace UserApiTestTask.Application.Common.Exceptions;

/// <summary>
/// Сущность не найдена
/// </summary>
public class EntityNotFoundProblem<TEntity> : ApplicationProblem
	where TEntity : EntityBase
{

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="message">Сообщение</param>
	public EntityNotFoundProblem(string message) : base(message)
	{ }

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="field">Поле</param>
	/// <param name="value">Значение</param>
	public EntityNotFoundProblem(string field, string value)
		: base($"Не удалось найти сущность '{typeof(TEntity).Name}' " +
			$"по полю '{field}', которое = '{value}'")
	{ }

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="id">Идентификатор</param>
	public EntityNotFoundProblem(Guid id)
		: base($"Не удалось найти сущность '{typeof(TEntity).Name}' по id = '{id}'")
	{ }

	/// <inheritdoc/>
	public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}
