using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PaperSite.Infrastructure.Persistence;

#nullable disable

namespace PaperSite.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260827083000_FixOtpSchema")]
public partial class FixOtpSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.OtpCodes', N'PhoneNumber') IS NULL
            BEGIN
                ALTER TABLE [OtpCodes] ADD [PhoneNumber] nvarchar(20) NULL;
            END
            """);

        // This must be a separate SQL command. SQL Server compiles a batch before
        // executing ALTER TABLE, so referencing the new column in the same batch fails.
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.OtpCodes', N'UserId') IS NOT NULL
            BEGIN
                UPDATE otp
                SET [PhoneNumber] = COALESCE(users.[PhoneNumber], N'')
                FROM [OtpCodes] AS otp
                LEFT JOIN [Users] AS users ON users.[Id] = otp.[UserId]
                WHERE otp.[PhoneNumber] IS NULL;
            END
            """);

        migrationBuilder.Sql(
            """
            UPDATE [OtpCodes]
            SET [PhoneNumber] = N''
            WHERE [PhoneNumber] IS NULL;

            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE [object_id] = OBJECT_ID(N'dbo.OtpCodes')
                  AND [name] = N'PhoneNumber'
                  AND [is_nullable] = 1)
            BEGIN
                ALTER TABLE [OtpCodes] ALTER COLUMN [PhoneNumber] nvarchar(20) NOT NULL;
            END
            """);

        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.OtpCodes', N'FailedAttempts') IS NULL
            BEGIN
                ALTER TABLE [OtpCodes]
                ADD [FailedAttempts] int NOT NULL
                    CONSTRAINT [DF_OtpCodes_FailedAttempts] DEFAULT 0;
            END

            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys
                WHERE [name] = N'FK_OtpCodes_Users_UserId'
                  AND [parent_object_id] = OBJECT_ID(N'dbo.OtpCodes'))
            BEGIN
                ALTER TABLE [OtpCodes] DROP CONSTRAINT [FK_OtpCodes_Users_UserId];
            END

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_OtpCodes_UserId'
                  AND [object_id] = OBJECT_ID(N'dbo.OtpCodes'))
            BEGIN
                DROP INDEX [IX_OtpCodes_UserId] ON [OtpCodes];
            END

            IF COL_LENGTH(N'dbo.OtpCodes', N'UserId') IS NOT NULL
            BEGIN
                ALTER TABLE [OtpCodes] DROP COLUMN [UserId];
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [name] = N'IX_OtpCodes_PhoneNumber_CreatedAt'
                  AND [object_id] = OBJECT_ID(N'dbo.OtpCodes'))
            BEGIN
                CREATE INDEX [IX_OtpCodes_PhoneNumber_CreatedAt]
                ON [OtpCodes] ([PhoneNumber], [CreatedAt]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This migration reconciles databases created from an older OTP model.
        // OTP records are temporary, so schema rollback is intentionally not destructive.
    }
}
