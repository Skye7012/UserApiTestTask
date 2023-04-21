using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Persistence.Common;

namespace UserApiTestTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация для <see cref="User"/>
/// </summary>
public class UserConfiguration : EntityBaseConfiguration<User>
{
	/// <inheritdoc/>
	public override void ConfigureChild(EntityTypeBuilder<User> builder)
	{
		builder.HasComment("Пользователи");

		builder.Property(e => e.Login);
		builder.Property(e => e.PasswordHash);
		builder.Property(e => e.PasswordSalt);
		builder.Property(e => e.Name);
		builder.Property(e => e.Gender);
		builder.Property(e => e.BirthDay);
		builder.Property(e => e.IsAdmin);

		builder.Property(e => e.CreatedBy);
		builder.Property(e => e.CreatedOn);
		builder.Property(e => e.ModifiedBy);
		builder.Property(e => e.ModifiedOn);
		builder.Property(e => e.RevokedBy);
		builder.Property(e => e.RevokedOn);

		builder.HasIndex(e => e.Login)
			.IsUnique();
	}
}
