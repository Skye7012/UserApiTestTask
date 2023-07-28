using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApiTestTask.Domain.Entities.Common;

namespace UserApiTestTask.Infrastructure.Extensions;

/// <summary>
/// Расширения для конфигурации сущностей, реализующих <see cref="ISoftDeletable"/>
/// </summary>
public static class ISoftDeletableTypeBuilderExtensions
{
	/// <summary>
	/// Добавить атрибуты из <see cref="ISoftDeletable"/>
	/// </summary>
	/// <param name="entityType">Тип сущности</param>
	public static void AddISoftDeletableFields(this IMutableEntityType entityType)
	{
		var builder = new EntityTypeBuilder<ISoftDeletable>(entityType);

		builder.Property(x => x.RevokedOn)
			.HasComment("Дата удаления");

		builder.Property(x => x.RevokedBy)
			.HasComment("Логин Пользователя, удалившего сущность");
	}
}
