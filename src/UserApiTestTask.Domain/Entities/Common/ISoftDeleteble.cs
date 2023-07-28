namespace UserApiTestTask.Domain.Entities.Common;

/// <summary>
/// Сущность, поддерживающая мягкое удаление
/// </summary>
public interface ISoftDeletable
{
	/// <summary>
	/// Дата удаления
	/// </summary>
	public DateTime? RevokedOn { get; set; }

	/// <summary>
	/// Логин Пользователя, удалившего сущность
	/// </summary>
	public string? RevokedBy { get; set; }
}
