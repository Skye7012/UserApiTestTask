using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserApiTestTask.Infrastructure.Persistence.Migrations
{
	public partial class init : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "user_accounts",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
					login = table.Column<string>(type: "text", nullable: false),
					password_hash = table.Column<byte[]>(type: "bytea", nullable: false),
					password_salt = table.Column<byte[]>(type: "bytea", nullable: false),
					revoked_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					revoked_by = table.Column<string>(type: "text", nullable: true),
					created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin"),
					modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					modified_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin")
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_accounts", x => x.id);
				},
				comment: "Аккаунт пользователя");

			migrationBuilder.CreateTable(
				name: "refresh_tokens",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
					token = table.Column<string>(type: "text", nullable: false),
					revoked_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					revoked_by = table.Column<string>(type: "text", nullable: true),
					user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
					created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin"),
					modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					modified_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin")
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_refresh_tokens", x => x.id);
					table.ForeignKey(
						name: "fk_refresh_tokens_user_accounts_user_account_id",
						column: x => x.user_account_id,
						principalTable: "user_accounts",
						principalColumn: "id");
				},
				comment: "Refresh токен");

			migrationBuilder.CreateTable(
				name: "users",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
					name = table.Column<string>(type: "text", nullable: false),
					gender = table.Column<int>(type: "integer", nullable: false),
					birth_day = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					is_admin = table.Column<bool>(type: "boolean", nullable: false),
					revoked_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					revoked_by = table.Column<string>(type: "text", nullable: true),
					user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
					created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin"),
					modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
					modified_by = table.Column<string>(type: "text", nullable: false, defaultValue: "Admin")
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_users", x => x.id);
					table.ForeignKey(
						name: "fk_users_user_accounts_user_account_id",
						column: x => x.user_account_id,
						principalTable: "user_accounts",
						principalColumn: "id");
				},
				comment: "Пользователи");

			migrationBuilder.CreateIndex(
				name: "ix_refresh_tokens_token",
				table: "refresh_tokens",
				column: "token",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_refresh_tokens_user_account_id",
				table: "refresh_tokens",
				column: "user_account_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_accounts_login",
				table: "user_accounts",
				column: "login",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_users_user_account_id",
				table: "users",
				column: "user_account_id",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "refresh_tokens");

			migrationBuilder.DropTable(
				name: "users");

			migrationBuilder.DropTable(
				name: "user_accounts");
		}
	}
}
