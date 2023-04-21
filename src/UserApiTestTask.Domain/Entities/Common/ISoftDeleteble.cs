namespace UserApiTestTask.Domain.Entities.Common;

/// <summary>
/// Сущность, поддерживающая мягкое удаление
/// </summary>
public interface ISoftDeletable
{
	/// <summary>
	/// Дата удаления пользователя
	/// </summary>
	public DateTime? RevokedOn { get; set; }

	/// <summary>
	/// Логин Пользователя, от имени которого этот пользователь удалён
	/// </summary>
	public string? RevokedBy { get; set; }
}
