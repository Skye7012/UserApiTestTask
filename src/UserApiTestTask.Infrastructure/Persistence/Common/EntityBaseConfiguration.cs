using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using UserApiTestTask.Domain.Entities.Common;
using UserApiTestTask.Infrastructure.InitExecutors;

namespace UserApiTestTask.Infrastructure.Persistence.Common;

/// <summary>
/// Конфигурация для сущностей, которые наследуются от <see cref="EntityBase"/>
/// </summary>
public abstract class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
	where TEntity : EntityBase
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<TEntity> builder)
	{
		ConfigureBase(builder);
		ConfigureChild(builder);
	}

	/// <summary>
	/// Сконфигурировать базовые поля из <see cref="EntityBase"/>
	/// </summary>
	/// <param name="builder">Билдер</param>
	private static void ConfigureBase(EntityTypeBuilder<TEntity> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.HasValueGenerator<GuidValueGenerator>()
			.HasDefaultValueSql("gen_random_uuid()");

		builder.Property(e => e.CreatedBy)
			.HasDefaultValue(DbInitExecutor.AdminLogin);
		builder.Property(e => e.CreatedOn)
			.HasDefaultValueSql("now()");

		builder.Property(e => e.ModifiedBy)
			.HasDefaultValue(DbInitExecutor.AdminLogin);
		builder.Property(e => e.ModifiedOn)
			.HasDefaultValueSql("now()");
	}

	/// <summary>
	/// Сконфигурировать <typeparamref name="TEntity" /> которая является дочерней от <see cref="EntityBase"/>
	/// </summary>
	/// <param name="builder">Билдер</param>
	public abstract void ConfigureChild(EntityTypeBuilder<TEntity> builder);
}
