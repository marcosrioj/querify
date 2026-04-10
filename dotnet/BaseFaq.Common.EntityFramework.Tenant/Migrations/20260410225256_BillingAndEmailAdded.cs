using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseFaq.Common.EntityFramework.Tenant.Migrations
{
    /// <inheritdoc />
    public partial class BillingAndEmailAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalCustomerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    LastEventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingWebhookInboxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SignatureValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsLiveMode = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderAccountId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReceivedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedUntilDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingWebhookInboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailOutboxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    HtmlBody = table.Column<string>(type: "text", nullable: false),
                    TextBody = table.Column<string>(type: "text", nullable: true),
                    FromEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    FromName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    QueuedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedUntilDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantEntitlementSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubscriptionStatus = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsInGracePeriod = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FeatureJson = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantEntitlementSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BillingInterval = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    TrialEndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GraceUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DefaultProvider = table.Column<int>(type: "integer", nullable: false),
                    CancelAtPeriodEnd = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalInvoiceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HostedUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PdfUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    LastEventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingInvoices_TenantSubscriptions_TenantSubscriptionId",
                        column: x => x.TenantSubscriptionId,
                        principalTable: "TenantSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BillingProviderSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalSubscriptionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExternalPriceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExternalProductId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentPeriodStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelAtPeriodEnd = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingProviderSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingProviderSubscriptions_TenantSubscriptions_TenantSubs~",
                        column: x => x.TenantSubscriptionId,
                        principalTable: "TenantSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalPaymentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Method = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEventCreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingPayments_BillingInvoices_BillingInvoiceId",
                        column: x => x.BillingInvoiceId,
                        principalTable: "BillingInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomer_IsDeleted",
                table: "BillingCustomers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomer_Provider_ExternalCustomerId",
                table: "BillingCustomers",
                columns: new[] { "Provider", "ExternalCustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomer_TenantId",
                table: "BillingCustomers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomer_TenantId_Provider",
                table: "BillingCustomers",
                columns: new[] { "TenantId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoice_IsDeleted",
                table: "BillingInvoices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoice_Provider_ExternalInvoiceId",
                table: "BillingInvoices",
                columns: new[] { "Provider", "ExternalInvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoice_TenantId",
                table: "BillingInvoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoice_TenantId_Status_PaidAtUtc",
                table: "BillingInvoices",
                columns: new[] { "TenantId", "Status", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_TenantSubscriptionId",
                table: "BillingInvoices",
                column: "TenantSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_IsDeleted",
                table: "BillingPayments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_Provider_ExternalPaymentId",
                table: "BillingPayments",
                columns: new[] { "Provider", "ExternalPaymentId" },
                unique: true,
                filter: "\"ExternalPaymentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_TenantId",
                table: "BillingPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_TenantId_Status_PaidAtUtc",
                table: "BillingPayments",
                columns: new[] { "TenantId", "Status", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayments_BillingInvoiceId",
                table: "BillingPayments",
                column: "BillingInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingProviderSubscription_IsDeleted",
                table: "BillingProviderSubscriptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BillingProviderSubscription_Provider_ExternalSubscriptionId",
                table: "BillingProviderSubscriptions",
                columns: new[] { "Provider", "ExternalSubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingProviderSubscription_TenantId",
                table: "BillingProviderSubscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingProviderSubscription_TenantId_TenantSubscriptionId",
                table: "BillingProviderSubscriptions",
                columns: new[] { "TenantId", "TenantSubscriptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingProviderSubscriptions_TenantSubscriptionId",
                table: "BillingProviderSubscriptions",
                column: "TenantSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingWebhookInbox_IsDeleted",
                table: "BillingWebhookInboxes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BillingWebhookInbox_Provider_ExternalEventId",
                table: "BillingWebhookInboxes",
                columns: new[] { "Provider", "ExternalEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingWebhookInbox_ReceivedDateUtc",
                table: "BillingWebhookInboxes",
                column: "ReceivedDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_BillingWebhookInbox_Status_NextAttempt_LockedUntil",
                table: "BillingWebhookInboxes",
                columns: new[] { "Status", "NextAttemptDateUtc", "LockedUntilDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingWebhookInbox_TenantId",
                table: "BillingWebhookInboxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_IsDeleted",
                table: "EmailOutboxes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_QueuedDateUtc",
                table: "EmailOutboxes",
                column: "QueuedDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_Status_NextAttempt_LockedUntil",
                table: "EmailOutboxes",
                columns: new[] { "Status", "NextAttemptDateUtc", "LockedUntilDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEntitlementSnapshot_IsActive_IsInGracePeriod_EffectiveUntilUtc",
                table: "TenantEntitlementSnapshots",
                columns: new[] { "IsActive", "IsInGracePeriod", "EffectiveUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantEntitlementSnapshot_IsDeleted",
                table: "TenantEntitlementSnapshots",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TenantEntitlementSnapshot_TenantId",
                table: "TenantEntitlementSnapshots",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscription_IsDeleted",
                table: "TenantSubscriptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscription_Status_CurrentPeriodEndUtc",
                table: "TenantSubscriptions",
                columns: new[] { "Status", "CurrentPeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscription_TenantId",
                table: "TenantSubscriptions",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingCustomers");

            migrationBuilder.DropTable(
                name: "BillingPayments");

            migrationBuilder.DropTable(
                name: "BillingProviderSubscriptions");

            migrationBuilder.DropTable(
                name: "BillingWebhookInboxes");

            migrationBuilder.DropTable(
                name: "EmailOutboxes");

            migrationBuilder.DropTable(
                name: "TenantEntitlementSnapshots");

            migrationBuilder.DropTable(
                name: "BillingInvoices");

            migrationBuilder.DropTable(
                name: "TenantSubscriptions");
        }
    }
}
