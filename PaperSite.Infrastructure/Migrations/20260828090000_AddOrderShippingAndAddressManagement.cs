using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PaperSite.Infrastructure.Persistence;

#nullable disable

namespace PaperSite.Infrastructure.Migrations;

/// <summary>
/// ستون‌های جدید سفارش (روش ارسال، رفرنس آدرس) و هم‌سطح‌سازی جدول UserAddresses
/// با موجودیت جدید (soft delete و ستون‌های زمانی).
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260828090000_AddOrderShippingAndAddressManagement")]
public partial class AddOrderShippingAndAddressManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.UserAddresses', N'U') IS NULL
            BEGIN
                CREATE TABLE [UserAddresses] (
                    [Id] uniqueidentifier NOT NULL,
                    [UserId] uniqueidentifier NOT NULL,
                    [Title] nvarchar(50) NOT NULL,
                    [Province] nvarchar(50) NOT NULL,
                    [City] nvarchar(60) NOT NULL,
                    [FullAddress] nvarchar(400) NOT NULL,
                    [PostalCode] nvarchar(10) NOT NULL,
                    [IsDefault] bit NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_UserAddresses_Users_UserId] FOREIGN KEY ([UserId])
                        REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                );
            END
            """);

        // ستون‌های ممکن است در دیتابیس‌های قدیمی‌تر وجود نداشته باشند.
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.UserAddresses', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.UserAddresses', N'CreatedAt') IS NULL
                    ALTER TABLE [UserAddresses] ADD [CreatedAt] datetime2 NOT NULL
                        CONSTRAINT [DF_UserAddresses_CreatedAt] DEFAULT '0001-01-01T00:00:00.0000000';

                IF COL_LENGTH(N'dbo.UserAddresses', N'UpdatedAt') IS NULL
                    ALTER TABLE [UserAddresses] ADD [UpdatedAt] datetime2 NOT NULL
                        CONSTRAINT [DF_UserAddresses_UpdatedAt] DEFAULT '0001-01-01T00:00:00.0000000';

                IF COL_LENGTH(N'dbo.UserAddresses', N'IsDeleted') IS NULL
                    ALTER TABLE [UserAddresses] ADD [IsDeleted] bit NOT NULL
                        CONSTRAINT [DF_UserAddresses_IsDeleted] DEFAULT 0;
            END
            """);

        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.UserAddresses', N'U') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = N'IX_UserAddresses_UserId_IsDefault_CreatedAt'
                      AND [object_id] = OBJECT_ID(N'dbo.UserAddresses'))
            BEGIN
                CREATE INDEX [IX_UserAddresses_UserId_IsDefault_CreatedAt]
                ON [UserAddresses] ([UserId], [IsDefault], [CreatedAt]);
            END
            """);

        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Orders', N'ShippingMethod') IS NULL
                    ALTER TABLE [Orders] ADD [ShippingMethod] int NOT NULL
                        CONSTRAINT [DF_Orders_ShippingMethod] DEFAULT 2;

                IF COL_LENGTH(N'dbo.Orders', N'AddressId') IS NULL
                    ALTER TABLE [Orders] ADD [AddressId] uniqueidentifier NULL;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Orders', N'AddressId') IS NOT NULL
                    ALTER TABLE [Orders] DROP COLUMN [AddressId];

                IF COL_LENGTH(N'dbo.Orders', N'ShippingMethod') IS NOT NULL
                BEGIN
                    ALTER TABLE [Orders] DROP CONSTRAINT [DF_Orders_ShippingMethod];
                    ALTER TABLE [Orders] DROP COLUMN [ShippingMethod];
                END
            END
            """);
    }
}