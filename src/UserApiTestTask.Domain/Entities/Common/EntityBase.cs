namespace UserApiTestTask.Domain.Entities.Common;

/// <summary>
/// Базовая сущность
/// </summary>
public abstract class EntityBase
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Дата создания пользователя
	/// </summary>
	public DateTime CreatedOn { get; set; }

	/// <summary>
	/// Логин Пользователя, от имени которого этот пользователь создан
	/// </summary>
	public string CreatedBy { get; set; } = default!;

	/// <summary>
	/// Дата изменения пользователя
	/// </summary>
	public DateTime ModifiedOn { get; set; }

	/// <summary>
	/// Логин Пользователя, от имени которого этот пользователь изменён
	/// </summary>
	public string ModifiedBy { get; set; } = default!;
}
