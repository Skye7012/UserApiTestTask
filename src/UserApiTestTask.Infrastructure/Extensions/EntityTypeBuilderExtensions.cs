using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserApiTestTask.Infrastructure.Extensions;

/// <summary>
/// Расширения для <see cref="EntityTypeBuilder"/>
/// </summary>
public static class EntityTypeBuilderExtensions
{
	/// <summary>
	/// Назначить резервное поле навигационному свойству
	/// </summary>
	/// <param name="source">Информация о пользователе</param>
	/// <param name="fieldName">Наименование резервного поля</param>
	/// <returns>Идентификатор пользователя</returns>
	public static ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity>
		HasField<TPrincipalEntity, TDependentEntity>(
			this ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> source,
			string fieldName)
			where TDependentEntity : class
			where TPrincipalEntity : class
	{
		source.Metadata.PrincipalToDependent!
			.SetField(fieldName);

		return source;
	}
}
