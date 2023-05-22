using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApiTestTask.Domain.Entities;
using UserApiTestTask.Infrastructure.Extensions;
using UserApiTestTask.Infrastructure.Persistence.Common;

namespace UserApiTestTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация для <see cref="RefreshToken"/>
/// </summary>
public class RefreshTokenConfiguration : EntityBaseConfiguration<RefreshToken>
{
	/// <inheritdoc/>
	public override void ConfigureChild(EntityTypeBuilder<RefreshToken> builder)
	{
		builder.HasComment("Refresh токен");

		builder.Property(e => e.Token);

		builder.Property(e => e.RevokedBy);
		builder.Property(e => e.RevokedOn);

		builder.HasIndex(e => e.Token)
			.IsUnique();

		builder.HasOne(e => e.UserAccount)
			.WithMany(e => e.RefreshTokens)
			.HasForeignKey(e => e.UserAccountId)
			.HasPrincipalKey(e => e.Id)
			.OnDelete(DeleteBehavior.ClientCascade)
			.HasField(UserAccount.RefreshTokensFieldName);
	}
}
