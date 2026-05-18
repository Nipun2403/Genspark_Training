using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "fine_config",
                columns: table => new
                {
                    fine_config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fine_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_unpaid_fine_threshold = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fine_config", x => x.fine_config_id);
                });

            migrationBuilder.CreateTable(
                name: "membership_config",
                columns: table => new
                {
                    config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    membership_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    max_active_borrowings = table.Column<int>(type: "integer", nullable: false),
                    max_borrow_days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_config", x => x.config_id);
                    table.UniqueConstraint("AK_membership_config_membership_type", x => x.membership_type);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.isbn);
                    table.ForeignKey(
                        name: "FK_books_book_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "book_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    member_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    membership_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    join_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.member_id);
                    table.ForeignKey(
                        name: "FK_members_membership_config_membership_type",
                        column: x => x.membership_type,
                        principalTable: "membership_config",
                        principalColumn: "membership_type",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "book_copies",
                columns: table => new
                {
                    copy_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_copies", x => x.copy_id);
                    table.CheckConstraint("chk_book_copy_status", "status IN ('Available', 'Borrowed', 'MinorDamage', 'DamagedBeyondUsable', 'Lost')");
                    table.ForeignKey(
                        name: "FK_book_copies_books_isbn",
                        column: x => x.isbn,
                        principalTable: "books",
                        principalColumn: "isbn",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "borrowings",
                columns: table => new
                {
                    borrowing_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    copy_id = table.Column<int>(type: "integer", nullable: false),
                    borrow_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    condition_at_borrow = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    condition_at_return = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_borrowings", x => x.borrowing_id);
                    table.ForeignKey(
                        name: "FK_borrowings_book_copies_copy_id",
                        column: x => x.copy_id,
                        principalTable: "book_copies",
                        principalColumn: "copy_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_borrowings_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fines",
                columns: table => new
                {
                    fine_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    borrowing_id = table.Column<int>(type: "integer", nullable: false),
                    fine_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fines", x => x.fine_id);
                    table.ForeignKey(
                        name: "FK_fines_borrowings_borrowing_id",
                        column: x => x.borrowing_id,
                        principalTable: "borrowings",
                        principalColumn: "borrowing_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fines_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fine_payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fine_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fine_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_fine_payments_fines_fine_id",
                        column: x => x.fine_id,
                        principalTable: "fines",
                        principalColumn: "fine_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "fine_config",
                columns: new[] { "fine_config_id", "amount", "fine_type", "max_unpaid_fine_threshold" },
                values: new object[,]
                {
                    { 1, 10.00m, "LateReturn", 500.00m },
                    { 2, 200.00m, "MinorDamage", 500.00m },
                    { 3, 500.00m, "DamagedBeyondUsable", 500.00m },
                    { 4, 1000.00m, "Lost", 500.00m }
                });

            migrationBuilder.InsertData(
                table: "membership_config",
                columns: new[] { "config_id", "max_active_borrowings", "max_borrow_days", "membership_type" },
                values: new object[,]
                {
                    { 1, 2, 7, "Basic" },
                    { 2, 3, 10, "Student" },
                    { 3, 5, 15, "Premium" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_book_categories_category_name",
                table: "book_categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_book_copies_isbn",
                table: "book_copies",
                column: "isbn");

            migrationBuilder.CreateIndex(
                name: "idx_books_author",
                table: "books",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "idx_books_title",
                table: "books",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_books_category_id",
                table: "books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_borrowings_copy_id",
                table: "borrowings",
                column: "copy_id");

            migrationBuilder.CreateIndex(
                name: "IX_borrowings_member_id",
                table: "borrowings",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_fine_config_fine_type",
                table: "fine_config",
                column: "fine_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fine_payments_fine_id",
                table: "fine_payments",
                column: "fine_id");

            migrationBuilder.CreateIndex(
                name: "IX_fines_borrowing_id",
                table: "fines",
                column: "borrowing_id");

            migrationBuilder.CreateIndex(
                name: "IX_fines_member_id",
                table: "fines",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "idx_members_email",
                table: "members",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_members_phone",
                table: "members",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_membership_type",
                table: "members",
                column: "membership_type");

            migrationBuilder.CreateIndex(
                name: "IX_membership_config_membership_type",
                table: "membership_config",
                column: "membership_type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fine_config");

            migrationBuilder.DropTable(
                name: "fine_payments");

            migrationBuilder.DropTable(
                name: "fines");

            migrationBuilder.DropTable(
                name: "borrowings");

            migrationBuilder.DropTable(
                name: "book_copies");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "membership_config");

            migrationBuilder.DropTable(
                name: "book_categories");
        }
    }
}
