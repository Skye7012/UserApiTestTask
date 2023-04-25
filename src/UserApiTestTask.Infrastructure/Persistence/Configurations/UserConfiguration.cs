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

		builder.Property(e => e.Name);
		builder.Property(e => e.Gender);
		builder.Property(e => e.BirthDay);
		builder.Property(e => e.IsAdmin);

		builder.Property(e => e.RevokedBy);
		builder.Property(e => e.RevokedOn);

		builder.HasOne(e => e.UserAccount)
			.WithOne(e => e.User)
			.HasForeignKey<User>(e => e.UserAccountId)
			.HasPrincipalKey<UserAccount>(e => e.Id)
			.OnDelete(DeleteBehavior.ClientCascade);
	}
}
