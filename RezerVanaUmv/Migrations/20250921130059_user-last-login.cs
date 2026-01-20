using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RezerVanaUmv.Migrations
{
    /// <inheritdoc />
    public partial class userlastlogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "agencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    iletisim_alan_kodu = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    iletisim_tel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    whatsapp_alan_kodu = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    whatsapp_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    viber_alan_kodu = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    viber_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unvan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ulke = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sehir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sirket_tel_alan_kodu = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sirket_tel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sirket_ulke = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sirket_sehir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sirket_adres = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("agencies_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPasswordChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DavetKoduTablosu",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DavetKodu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    AgencyId = table.Column<int>(type: "integer", nullable: true),
                    RoleId = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_davet_kodu_tablosu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subdomain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenants_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClaimValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "balance_points",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    agency_id = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("balance_points_pkey", x => x.id);
                    table.ForeignKey(
                        name: "balance_points_agency_id_fkey",
                        column: x => x.agency_id,
                        principalTable: "agencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "balance_points_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    multiplier = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true, defaultValueSql: "1.0"),
                    target_room_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("campaigns_pkey", x => x.id);
                    table.ForeignKey(
                        name: "campaigns_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "operators",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operators", x => x.id);
                    table.ForeignKey(
                        name: "FK_operators_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "point_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    agency_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    points = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    reservation_id = table.Column<int>(type: "integer", nullable: true),
                    redemption_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("point_transactions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "point_transactions_agency_id_fkey",
                        column: x => x.agency_id,
                        principalTable: "agencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "point_transactions_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "redemptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    required_points = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    room_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    use_earning_period = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("redemptions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "redemptions_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reservation_bonus_settings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bonus_procedure_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bonus_info_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    search_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "select"),
                    min_stay_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    max_stay_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    min_reservation_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_reservation_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 365),
                    min_balance = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    yearly_use_point = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    is_bonus_proc_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_excheckin_date_control = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservation_bonus_settings", x => x.id);
                    table.ForeignKey(
                        name: "reservation_bonus_settings_tenants_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    agency_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    operator_id = table.Column<int>(type: "integer", nullable: true),
                    checkin_date = table.Column<DateOnly>(type: "date", nullable: true),
                    checkout_date = table.Column<DateOnly>(type: "date", nullable: true),
                    room_count = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    room_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_amount = table.Column<int>(type: "integer", nullable: true),
                    booking_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reservation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValueSql: "'confirmed'::character varying"),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    night_count = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reservations_pkey", x => x.id);
                    table.ForeignKey(
                        name: "reservations_agency_id_fkey",
                        column: x => x.agency_id,
                        principalTable: "agencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "reservations_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reward_catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    required_points = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    use_earning_period = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    room_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reward_catalog_pkey", x => x.id);
                    table.ForeignKey(
                        name: "reward_catalog_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "roomtypes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    bedcount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    pricepernight = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    tenant_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roomtypes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "roomtypes_tenants_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_points",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    reservation_id = table.Column<int>(type: "integer", nullable: true),
                    agency_id = table.Column<int>(type: "integer", nullable: true),
                    base_points = table.Column<int>(type: "integer", nullable: false),
                    bonus_points = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    total_points = table.Column<int>(type: "integer", nullable: true, computedColumnSql: "(base_points + bonus_points)", stored: true),
                    calculated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("loyalty_points_pkey", x => x.id);
                    table.ForeignKey(
                        name: "loyalty_points_agency_id_fkey",
                        column: x => x.agency_id,
                        principalTable: "agencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "loyalty_points_reservation_id_fkey",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "loyalty_points_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "passengers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    reservation_id = table.Column<int>(type: "integer", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("passengers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_passengers_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "passengers_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "passenger_points",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: true),
                    passenger_id = table.Column<int>(type: "integer", nullable: true),
                    reservation_id = table.Column<int>(type: "integer", nullable: true),
                    points = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'earned'::character varying"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("passenger_points_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_passenger_points_passengers_passenger_id",
                        column: x => x.passenger_id,
                        principalTable: "passengers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "passenger_points_reservation_id_fkey",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "passenger_points_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_balance_points_agency_id",
                table: "balance_points",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_balance_points_tenant_id",
                table: "balance_points",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_tenant_id",
                table: "campaigns",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalty_points_agency_id",
                table: "loyalty_points",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalty_points_reservation_id",
                table: "loyalty_points",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_loyalty_points_tenant_id",
                table: "loyalty_points",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_operators_tenant_id",
                schema: "public",
                table: "operators",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_passenger_points_passenger_id",
                table: "passenger_points",
                column: "passenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_passenger_points_reservation_id",
                table: "passenger_points",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_passenger_points_tenant_id",
                table: "passenger_points",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_passengers_reservation_id",
                table: "passengers",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_passengers_tenant_id",
                table: "passengers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_agency_id",
                table: "point_transactions",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_tenant_id",
                table: "point_transactions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_redemptions_tenant_id",
                table: "redemptions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_redemptions_room_period",
                table: "redemptions",
                columns: new[] { "room_type", "start_date", "end_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_bonus_settings_tenant_id",
                schema: "public",
                table: "reservation_bonus_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_agency_id",
                table: "reservations",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_tenant_id",
                table: "reservations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_catalog_tenant_id",
                table: "reward_catalog",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_reward_catalog_room_period",
                table: "reward_catalog",
                columns: new[] { "room_type", "start_date", "end_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roomtypes_tenant_id",
                schema: "public",
                table: "roomtypes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "tenants_subdomain_key",
                table: "tenants",
                column: "subdomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "balance_points");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "DavetKoduTablosu",
                schema: "public");

            migrationBuilder.DropTable(
                name: "loyalty_points");

            migrationBuilder.DropTable(
                name: "operators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "passenger_points");

            migrationBuilder.DropTable(
                name: "point_transactions");

            migrationBuilder.DropTable(
                name: "redemptions");

            migrationBuilder.DropTable(
                name: "reservation_bonus_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reward_catalog");

            migrationBuilder.DropTable(
                name: "roomtypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "passengers");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "agencies");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
