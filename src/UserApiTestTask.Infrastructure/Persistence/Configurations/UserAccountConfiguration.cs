using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Extensions;
using UserApiTestTask.Infrastructure.Persistence.Common;

namespace UserApiTestTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация для <see cref="UserAccount"/>
/// </summary>
public class UserAccountConfiguration : EntityBaseConfiguration<UserAccount>
{
	/// <inheritdoc/>
	public override void ConfigureChild(EntityTypeBuilder<UserAccount> builder)
	{
		builder.HasComment("Аккаунт пользователя");

		builder.Property(e => e.Login);
		builder.Property(e => e.PasswordHash);
		builder.Property(e => e.PasswordSalt);

		builder.Property(e => e.RevokedBy);
		builder.Property(e => e.RevokedOn);

		builder.HasIndex(e => e.Login)
			.IsUnique();

		builder.HasOne(e => e.User)
			.WithOne(e => e.UserAccount)
			.HasForeignKey<User>(e => e.UserAccountId)
			.HasPrincipalKey<UserAccount>(e => e.Id)
			.OnDelete(DeleteBehavior.ClientCascade);

		builder.HasMany(e => e.RefreshTokens)
			.WithOne(e => e.UserAccount)
			.HasForeignKey(e => e.UserAccountId)
			.HasPrincipalKey(e => e.Id)
			.OnDelete(DeleteBehavior.ClientCascade)
			.HasField(UserAccount.RefreshTokensFieldName);
	}
}
