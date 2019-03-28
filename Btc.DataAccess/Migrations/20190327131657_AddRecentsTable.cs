using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Btc.DataAccess.Migrations
{
    public partial class AddRecentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_RecentTransactions",
                        column: x => x.Id,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            string sp = @"ALTER TABLE [dbo].[RecentTransactions] REBUILD WITH (IGNORE_DUP_KEY = ON)";

            migrationBuilder.Sql(sp);

            // read recent transactions once then clean up the table
            sp = @"
                CREATE PROCEDURE [dbo].[GetLastTransactions]
                    @confirmation int
                AS
                BEGIN
                    SET NOCOUNT ON;
                    select t.* from Transactions t
                    inner join RecentTransactions rt
                    on t.Id = rt.Id
                    union
                    select t.* from Transactions t
                    where t.Confirmation < @confirmation

                    delete from RecentTransactions
                END";

            migrationBuilder.Sql(sp);

            // new transaction is going to be recent transaction
            sp = @"
                CREATE TRIGGER AddRecents  
                ON dbo.Transactions
                AFTER INSERT
                AS
                    insert into dbo.RecentTransactions ([Id])
                    select Id from inserted where inserted.TransactionType = 1
                GO  
            ";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.GetLastTransactions");
            migrationBuilder.DropTable(name: "RecentTransactions");
        }
    }
}
