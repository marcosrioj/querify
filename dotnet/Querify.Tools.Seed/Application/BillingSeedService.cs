using Querify.Common.EntityFramework.Core.Entities;
using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;
using Querify.Tools.Seed.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tools.Seed.Application;

public sealed class BillingSeedService : IBillingSeedService
{
    // Reference date — stable timestamps across seed runs
    private static readonly DateTime SeedNow = new(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedTenantSlug = "tenant-001";

    // Tenant IDs
    private static readonly Guid NorthPeakTenantId    = new("00000001-0001-0001-0001-000000000001");
    private static readonly Guid PacificTrailTenantId  = new("00000001-0001-0001-0001-000000000002");
    private static readonly Guid MapleForgeId          = new("00000001-0001-0001-0001-000000000003");
    private static readonly Guid AuroraClinicId        = new("00000001-0001-0001-0001-000000000004");
    private static readonly Guid BlueHarborId          = new("00000001-0001-0001-0001-000000000005");

    // BillingCustomer IDs
    private static readonly Guid BcSeedTenantId   = new("00000002-0001-0001-0001-000000000010");
    private static readonly Guid BcNorthPeakId   = new("00000002-0001-0001-0001-000000000001");
    private static readonly Guid BcPacificId     = new("00000002-0001-0001-0001-000000000002");
    private static readonly Guid BcMapleForgeId  = new("00000002-0001-0001-0001-000000000003");
    private static readonly Guid BcAuroraId      = new("00000002-0001-0001-0001-000000000004");
    private static readonly Guid BcBlueHarborId  = new("00000002-0001-0001-0001-000000000005");

    // TenantSubscription IDs
    private static readonly Guid SubSeedTenantId   = new("00000003-0001-0001-0001-000000000010");
    private static readonly Guid SubNorthPeakId   = new("00000003-0001-0001-0001-000000000001");
    private static readonly Guid SubPacificId     = new("00000003-0001-0001-0001-000000000002");
    private static readonly Guid SubMapleForgeId  = new("00000003-0001-0001-0001-000000000003");
    private static readonly Guid SubAuroraId      = new("00000003-0001-0001-0001-000000000004");
    private static readonly Guid SubBlueHarborId  = new("00000003-0001-0001-0001-000000000005");

    // BillingProviderSubscription IDs
    private static readonly Guid PsSeedTenantId   = new("00000004-0001-0001-0001-000000000010");
    private static readonly Guid PsNorthPeakId   = new("00000004-0001-0001-0001-000000000001");
    private static readonly Guid PsPacificId     = new("00000004-0001-0001-0001-000000000002");
    private static readonly Guid PsMapleForgeId  = new("00000004-0001-0001-0001-000000000003");
    private static readonly Guid PsAuroraId      = new("00000004-0001-0001-0001-000000000004");
    private static readonly Guid PsBlueHarborId  = new("00000004-0001-0001-0001-000000000005");

    // BillingInvoice IDs
    private static readonly Guid InvSeedTenant1Id   = new("00000005-0001-0001-0001-000000000010");
    private static readonly Guid InvSeedTenant2Id   = new("00000005-0001-0001-0001-000000000011");
    private static readonly Guid InvNorthPeak1Id   = new("00000005-0001-0001-0001-000000000001");
    private static readonly Guid InvNorthPeak2Id   = new("00000005-0001-0001-0001-000000000002");
    private static readonly Guid InvMapleForge1Id  = new("00000005-0001-0001-0001-000000000003");
    private static readonly Guid InvMapleForge2Id  = new("00000005-0001-0001-0001-000000000004");
    private static readonly Guid InvAurora1Id      = new("00000005-0001-0001-0001-000000000005");
    private static readonly Guid InvBlueHarbor1Id  = new("00000005-0001-0001-0001-000000000006");

    // BillingPayment IDs
    private static readonly Guid PaySeedTenant1Id   = new("00000006-0001-0001-0001-000000000010");
    private static readonly Guid PaySeedTenant2Id   = new("00000006-0001-0001-0001-000000000011");
    private static readonly Guid PayNorthPeak1Id   = new("00000006-0001-0001-0001-000000000001");
    private static readonly Guid PayNorthPeak2Id   = new("00000006-0001-0001-0001-000000000002");
    private static readonly Guid PayMapleForge1Id  = new("00000006-0001-0001-0001-000000000003");
    private static readonly Guid PayMapleForge2Id  = new("00000006-0001-0001-0001-000000000004");
    private static readonly Guid PayAurora1Id      = new("00000006-0001-0001-0001-000000000005");
    private static readonly Guid PayBlueHarbor1Id  = new("00000006-0001-0001-0001-000000000006");

    // TenantEntitlementSnapshot IDs
    private static readonly Guid EntSeedTenantId   = new("00000007-0001-0001-0001-000000000010");
    private static readonly Guid EntNorthPeakId   = new("00000007-0001-0001-0001-000000000001");
    private static readonly Guid EntPacificId     = new("00000007-0001-0001-0001-000000000002");
    private static readonly Guid EntMapleForgeId  = new("00000007-0001-0001-0001-000000000003");
    private static readonly Guid EntAuroraId      = new("00000007-0001-0001-0001-000000000004");
    private static readonly Guid EntBlueHarborId  = new("00000007-0001-0001-0001-000000000005");

    // BillingWebhookInbox IDs
    private static readonly Guid Wh1Id = new("00000008-0001-0001-0001-000000000001");
    private static readonly Guid Wh2Id = new("00000008-0001-0001-0001-000000000002");
    private static readonly Guid Wh3Id = new("00000008-0001-0001-0001-000000000003");
    private static readonly Guid Wh4Id = new("00000008-0001-0001-0001-000000000004");
    private static readonly Guid Wh5Id = new("00000008-0001-0001-0001-000000000005");

    // EmailOutbox IDs
    private static readonly Guid Email1Id = new("00000009-0001-0001-0001-000000000001");
    private static readonly Guid Email2Id = new("00000009-0001-0001-0001-000000000002");
    private static readonly Guid Email3Id = new("00000009-0001-0001-0001-000000000003");

    public bool HasBillingData(TenantDbContext dbContext, Guid seedTenantId)
    {
        return HasSeedTenantBillingData(dbContext, seedTenantId) &&
               HasScenarioBillingData(dbContext);
    }

    public void SeedBillingData(TenantDbContext dbContext, Guid seedTenantId, string productConnectionString)
    {
        SeedPrimarySeedTenantScenario(dbContext, seedTenantId, productConnectionString);
        SeedScenarioA_NorthPeakAnalytics(dbContext, productConnectionString);
        SeedScenarioB_PacificTrailStudio(dbContext, productConnectionString);
        SeedScenarioC_MapleForgeMedia(dbContext, productConnectionString);
        SeedScenarioD_AuroraClinicSystems(dbContext, productConnectionString);
        SeedScenarioE_BlueHarborLegal(dbContext, productConnectionString);
        SeedWebhookInboxScenarios(dbContext);
        SeedEmailOutboxScenarios(dbContext);
        dbContext.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // Primary seed workspace — tenant-001: Starter monthly, Active
    // -------------------------------------------------------------------------

    private static void SeedPrimarySeedTenantScenario(
        TenantDbContext dbContext,
        Guid seedTenantId,
        string productConnectionString)
    {
        EnsureTenant(dbContext, seedTenantId,
            slug: SeedTenantSlug,
            name: "Querify Seed Workspace",
            edition: TenantEdition.Free,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcSeedTenantId,
            tenantId: seedTenantId,
            externalId: "cus_Q8fSeedTenant001",
            email: "user001@seed.querify.local",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-3));

        EnsureTenantSubscription(dbContext, SubSeedTenantId,
            tenantId: seedTenantId,
            planCode: "starter-monthly",
            interval: BillingIntervalType.Month,
            status: TenantSubscriptionStatus.Active,
            currency: "CAD",
            countryCode: "CA",
            periodStart: SeedNow.AddDays(-3),
            periodEnd: SeedNow.AddDays(27),
            lastEvent: SeedNow.AddDays(-3));

        EnsureBillingProviderSubscription(dbContext, PsSeedTenantId,
            tenantSubscriptionId: SubSeedTenantId,
            tenantId: seedTenantId,
            externalSubId: "sub_1RSeedTenant001",
            externalPriceId: "price_1RPriceStarterMonthly",
            externalProductId: "prod_1RProdStarterQuerify",
            status: TenantSubscriptionStatus.Active,
            periodStart: SeedNow.AddDays(-3),
            periodEnd: SeedNow.AddDays(27),
            lastEvent: SeedNow.AddDays(-3));

        EnsureBillingInvoice(dbContext, InvSeedTenant2Id,
            tenantSubscriptionId: SubSeedTenantId,
            tenantId: seedTenantId,
            externalInvoiceId: "in_1RSeedInvMar2026",
            amountMinor: 2900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-34),
            paidAt: SeedNow.AddDays(-34),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-34));

        EnsureBillingPayment(dbContext, PaySeedTenant2Id,
            invoiceId: InvSeedTenant2Id,
            tenantId: seedTenantId,
            externalPaymentId: "pi_3RSeedPay2026Mar",
            method: "card",
            amountMinor: 2900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-34),
            lastEvent: SeedNow.AddDays(-34));

        EnsureBillingInvoice(dbContext, InvSeedTenant1Id,
            tenantSubscriptionId: SubSeedTenantId,
            tenantId: seedTenantId,
            externalInvoiceId: "in_1RSeedInvApr2026",
            amountMinor: 2900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-3),
            paidAt: SeedNow.AddDays(-3),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-3));

        EnsureBillingPayment(dbContext, PaySeedTenant1Id,
            invoiceId: InvSeedTenant1Id,
            tenantId: seedTenantId,
            externalPaymentId: "pi_3RSeedPay2026Apr",
            method: "card",
            amountMinor: 2900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-3),
            lastEvent: SeedNow.AddDays(-3));

        EnsureTenantEntitlementSnapshot(dbContext, EntSeedTenantId,
            tenantId: seedTenantId,
            planCode: "starter-monthly",
            subscriptionStatus: TenantSubscriptionStatus.Active,
            isActive: true,
            isInGracePeriod: false,
            effectiveUntil: SeedNow.AddDays(27),
            featureJson: """{"maxSpaces":10,"maxQuestionsPerSpace":50,"aiGeneration":true,"analytics":false}""");
    }

    // -------------------------------------------------------------------------
    // Scenario A — NorthPeak Analytics: Pro monthly, Active, healthy billing
    // -------------------------------------------------------------------------

    private static void SeedScenarioA_NorthPeakAnalytics(TenantDbContext dbContext, string productConnectionString)
    {
        var tenant = EnsureTenant(dbContext, NorthPeakTenantId,
            slug: "northpeak-analytics",
            name: "NorthPeak Analytics",
            edition: TenantEdition.Pro,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcNorthPeakId,
            tenantId: NorthPeakTenantId,
            externalId: "cus_Q8fNPA1xY2ZaBcDe",
            email: "billing@northpeakanalytics.com",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-9));

        EnsureTenantSubscription(dbContext, SubNorthPeakId,
            tenantId: NorthPeakTenantId,
            planCode: "pro-monthly",
            interval: BillingIntervalType.Month,
            status: TenantSubscriptionStatus.Active,
            currency: "CAD",
            countryCode: "CA",
            periodStart: SeedNow.AddDays(-9),
            periodEnd: SeedNow.AddDays(21),
            lastEvent: SeedNow.AddDays(-9));

        EnsureBillingProviderSubscription(dbContext, PsNorthPeakId,
            tenantSubscriptionId: SubNorthPeakId,
            tenantId: NorthPeakTenantId,
            externalSubId: "sub_1RNPAbCdEfGhIjKlMnOp",
            externalPriceId: "price_1RPriceProMonthly",
            externalProductId: "prod_1RProdProQuerify",
            status: TenantSubscriptionStatus.Active,
            periodStart: SeedNow.AddDays(-9),
            periodEnd: SeedNow.AddDays(21),
            lastEvent: SeedNow.AddDays(-9));

        // Previous month invoice — paid
        EnsureBillingInvoice(dbContext, InvNorthPeak2Id,
            tenantSubscriptionId: SubNorthPeakId,
            tenantId: NorthPeakTenantId,
            externalInvoiceId: "in_1RNPInvMar2026",
            amountMinor: 9900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-40),
            paidAt: SeedNow.AddDays(-40),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-40));

        EnsureBillingPayment(dbContext, PayNorthPeak2Id,
            invoiceId: InvNorthPeak2Id,
            tenantId: NorthPeakTenantId,
            externalPaymentId: "pi_3RNPPay2026Mar",
            method: "card",
            amountMinor: 9900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-40),
            lastEvent: SeedNow.AddDays(-40));

        // Current month invoice — paid
        EnsureBillingInvoice(dbContext, InvNorthPeak1Id,
            tenantSubscriptionId: SubNorthPeakId,
            tenantId: NorthPeakTenantId,
            externalInvoiceId: "in_1RNPInvApr2026",
            amountMinor: 9900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-9),
            paidAt: SeedNow.AddDays(-9),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-9));

        EnsureBillingPayment(dbContext, PayNorthPeak1Id,
            invoiceId: InvNorthPeak1Id,
            tenantId: NorthPeakTenantId,
            externalPaymentId: "pi_3RNPPay2026Apr",
            method: "card",
            amountMinor: 9900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-9),
            lastEvent: SeedNow.AddDays(-9));

        EnsureTenantEntitlementSnapshot(dbContext, EntNorthPeakId,
            tenantId: NorthPeakTenantId,
            planCode: "pro-monthly",
            subscriptionStatus: TenantSubscriptionStatus.Active,
            isActive: true,
            isInGracePeriod: false,
            effectiveUntil: SeedNow.AddDays(21),
            featureJson: """{"maxSpaces":50,"maxQuestionsPerSpace":200,"aiGeneration":true,"analytics":true}""");
    }

    // -------------------------------------------------------------------------
    // Scenario B — Pacific Trail Studio: Starter monthly, Trialing
    // -------------------------------------------------------------------------

    private static void SeedScenarioB_PacificTrailStudio(TenantDbContext dbContext, string productConnectionString)
    {
        EnsureTenant(dbContext, PacificTrailTenantId,
            slug: "pacific-trail-studio",
            name: "Pacific Trail Studio",
            edition: TenantEdition.Starter,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcPacificId,
            tenantId: PacificTrailTenantId,
            externalId: "cus_Q8fPTS1xY2ZaBcDe",
            email: "ops@pacifictrailstudio.com",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-5));

        EnsureTenantSubscription(dbContext, SubPacificId,
            tenantId: PacificTrailTenantId,
            planCode: "starter-monthly",
            interval: BillingIntervalType.Month,
            status: TenantSubscriptionStatus.Trialing,
            currency: "CAD",
            countryCode: "CA",
            periodStart: SeedNow.AddDays(-5),
            periodEnd: SeedNow.AddDays(25),
            trialEndsAt: SeedNow.AddDays(35),
            lastEvent: SeedNow.AddDays(-5));

        EnsureBillingProviderSubscription(dbContext, PsPacificId,
            tenantSubscriptionId: SubPacificId,
            tenantId: PacificTrailTenantId,
            externalSubId: "sub_1RPTSAbCdEfGhIjKlMn",
            externalPriceId: "price_1RPriceStarterMonthly",
            externalProductId: "prod_1RProdStarterQuerify",
            status: TenantSubscriptionStatus.Trialing,
            periodStart: SeedNow.AddDays(-5),
            periodEnd: SeedNow.AddDays(25),
            trialEndsAt: SeedNow.AddDays(35),
            lastEvent: SeedNow.AddDays(-5));

        EnsureTenantEntitlementSnapshot(dbContext, EntPacificId,
            tenantId: PacificTrailTenantId,
            planCode: "starter-monthly",
            subscriptionStatus: TenantSubscriptionStatus.Trialing,
            isActive: true,
            isInGracePeriod: false,
            effectiveUntil: SeedNow.AddDays(35),
            featureJson: """{"maxSpaces":10,"maxQuestionsPerSpace":50,"aiGeneration":true,"analytics":false}""");
    }

    // -------------------------------------------------------------------------
    // Scenario C — MapleForge Media: Pro monthly, PastDue, grace period
    // -------------------------------------------------------------------------

    private static void SeedScenarioC_MapleForgeMedia(TenantDbContext dbContext, string productConnectionString)
    {
        EnsureTenant(dbContext, MapleForgeId,
            slug: "mapleforge-media",
            name: "MapleForge Media",
            edition: TenantEdition.Pro,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcMapleForgeId,
            tenantId: MapleForgeId,
            externalId: "cus_Q8fMFM1xY2ZaBcDe",
            email: "finance@mapleforge.io",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-5));

        EnsureTenantSubscription(dbContext, SubMapleForgeId,
            tenantId: MapleForgeId,
            planCode: "pro-monthly",
            interval: BillingIntervalType.Month,
            status: TenantSubscriptionStatus.PastDue,
            currency: "CAD",
            countryCode: "CA",
            periodStart: SeedNow.AddDays(-10),
            periodEnd: SeedNow.AddDays(20),
            graceUntil: SeedNow.AddDays(7),
            lastEvent: SeedNow.AddDays(-5));

        EnsureBillingProviderSubscription(dbContext, PsMapleForgeId,
            tenantSubscriptionId: SubMapleForgeId,
            tenantId: MapleForgeId,
            externalSubId: "sub_1RMFMAbCdEfGhIjKlMn",
            externalPriceId: "price_1RPriceProMonthly",
            externalProductId: "prod_1RProdProQuerify",
            status: TenantSubscriptionStatus.PastDue,
            periodStart: SeedNow.AddDays(-10),
            periodEnd: SeedNow.AddDays(20),
            lastEvent: SeedNow.AddDays(-5));

        // Previous month invoice — paid
        EnsureBillingInvoice(dbContext, InvMapleForge2Id,
            tenantSubscriptionId: SubMapleForgeId,
            tenantId: MapleForgeId,
            externalInvoiceId: "in_1RMFMInvMar2026",
            amountMinor: 9900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-40),
            paidAt: SeedNow.AddDays(-40),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-40));

        EnsureBillingPayment(dbContext, PayMapleForge2Id,
            invoiceId: InvMapleForge2Id,
            tenantId: MapleForgeId,
            externalPaymentId: "pi_3RMFMPay2026Mar",
            method: "card",
            amountMinor: 9900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-40),
            lastEvent: SeedNow.AddDays(-40));

        // Current invoice — payment failed
        EnsureBillingInvoice(dbContext, InvMapleForge1Id,
            tenantSubscriptionId: SubMapleForgeId,
            tenantId: MapleForgeId,
            externalInvoiceId: "in_1RMFMInvApr2026",
            amountMinor: 9900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-5),
            paidAt: null,
            status: BillingInvoiceStatus.Open,
            lastEvent: SeedNow.AddDays(-5));

        EnsureBillingPayment(dbContext, PayMapleForge1Id,
            invoiceId: InvMapleForge1Id,
            tenantId: MapleForgeId,
            externalPaymentId: "pi_3RMFMPay2026Apr",
            method: "card",
            amountMinor: 9900,
            currency: "CAD",
            status: BillingPaymentStatus.Failed,
            paidAt: null,
            failureCode: "card_declined",
            failureMessage: "Your card was declined. Please update your payment method to continue your Querify subscription.",
            lastEvent: SeedNow.AddDays(-5));

        EnsureTenantEntitlementSnapshot(dbContext, EntMapleForgeId,
            tenantId: MapleForgeId,
            planCode: "pro-monthly",
            subscriptionStatus: TenantSubscriptionStatus.PastDue,
            isActive: true,
            isInGracePeriod: true,
            effectiveUntil: SeedNow.AddDays(7),
            featureJson: """{"maxSpaces":50,"maxQuestionsPerSpace":200,"aiGeneration":true,"analytics":true}""");
    }

    // -------------------------------------------------------------------------
    // Scenario D — Aurora Clinic Systems: Pro yearly, Canceled
    // -------------------------------------------------------------------------

    private static void SeedScenarioD_AuroraClinicSystems(TenantDbContext dbContext, string productConnectionString)
    {
        EnsureTenant(dbContext, AuroraClinicId,
            slug: "aurora-clinic-systems",
            name: "Aurora Clinic Systems",
            edition: TenantEdition.Pro,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcAuroraId,
            tenantId: AuroraClinicId,
            externalId: "cus_Q8fACS1xY2ZaBcDe",
            email: "admin@auroraclinicsystems.com",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-9));

        EnsureTenantSubscription(dbContext, SubAuroraId,
            tenantId: AuroraClinicId,
            planCode: "pro-yearly",
            interval: BillingIntervalType.Year,
            status: TenantSubscriptionStatus.Canceled,
            currency: "CAD",
            countryCode: "CA",
            periodStart: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            periodEnd: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            cancelledAt: SeedNow.AddDays(-9),
            lastEvent: SeedNow.AddDays(-9));

        EnsureBillingProviderSubscription(dbContext, PsAuroraId,
            tenantSubscriptionId: SubAuroraId,
            tenantId: AuroraClinicId,
            externalSubId: "sub_1RACSAbCdEfGhIjKlMn",
            externalPriceId: "price_1RPriceProYearly",
            externalProductId: "prod_1RProdProQuerify",
            status: TenantSubscriptionStatus.Canceled,
            periodStart: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            periodEnd: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            cancelledAt: SeedNow.AddDays(-9),
            lastEvent: SeedNow.AddDays(-9));

        EnsureBillingInvoice(dbContext, InvAurora1Id,
            tenantSubscriptionId: SubAuroraId,
            tenantId: AuroraClinicId,
            externalInvoiceId: "in_1RACSInvApr2025",
            amountMinor: 118800,
            currency: "CAD",
            dueDate: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            paidAt: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            status: BillingInvoiceStatus.Paid,
            lastEvent: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        EnsureBillingPayment(dbContext, PayAurora1Id,
            invoiceId: InvAurora1Id,
            tenantId: AuroraClinicId,
            externalPaymentId: "pi_3RACSPay2025Apr",
            method: "card",
            amountMinor: 118800,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            lastEvent: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        EnsureTenantEntitlementSnapshot(dbContext, EntAuroraId,
            tenantId: AuroraClinicId,
            planCode: "pro-yearly",
            subscriptionStatus: TenantSubscriptionStatus.Canceled,
            isActive: false,
            isInGracePeriod: false,
            effectiveUntil: SeedNow.AddDays(-9),
            featureJson: """{"maxSpaces":50,"maxQuestionsPerSpace":200,"aiGeneration":true,"analytics":true}""");
    }

    // -------------------------------------------------------------------------
    // Scenario E — BlueHarbor Legal: Business monthly, Active (webhook demo tenant)
    // -------------------------------------------------------------------------

    private static void SeedScenarioE_BlueHarborLegal(TenantDbContext dbContext, string productConnectionString)
    {
        EnsureTenant(dbContext, BlueHarborId,
            slug: "blueharbor-legal",
            name: "BlueHarbor Legal",
            edition: TenantEdition.Business,
            productConnectionString: productConnectionString);

        EnsureBillingCustomer(dbContext, BcBlueHarborId,
            tenantId: BlueHarborId,
            externalId: "cus_Q8fBHL1xY2ZaBcDe",
            email: "it@blueharborlegal.com",
            countryCode: "CA",
            lastEvent: SeedNow.AddDays(-2));

        EnsureTenantSubscription(dbContext, SubBlueHarborId,
            tenantId: BlueHarborId,
            planCode: "business-monthly",
            interval: BillingIntervalType.Month,
            status: TenantSubscriptionStatus.Active,
            currency: "CAD",
            countryCode: "CA",
            periodStart: SeedNow.AddDays(-2),
            periodEnd: SeedNow.AddDays(28),
            lastEvent: SeedNow.AddDays(-2));

        EnsureBillingProviderSubscription(dbContext, PsBlueHarborId,
            tenantSubscriptionId: SubBlueHarborId,
            tenantId: BlueHarborId,
            externalSubId: "sub_1RBHLAbCdEfGhIjKlMn",
            externalPriceId: "price_1RPriceBusinessMonthly",
            externalProductId: "prod_1RProdBusinessQuerify",
            status: TenantSubscriptionStatus.Active,
            periodStart: SeedNow.AddDays(-2),
            periodEnd: SeedNow.AddDays(28),
            lastEvent: SeedNow.AddDays(-2));

        EnsureBillingInvoice(dbContext, InvBlueHarbor1Id,
            tenantSubscriptionId: SubBlueHarborId,
            tenantId: BlueHarborId,
            externalInvoiceId: "in_1RBHLInvApr2026",
            amountMinor: 29900,
            currency: "CAD",
            dueDate: SeedNow.AddDays(-2),
            paidAt: SeedNow.AddDays(-2),
            status: BillingInvoiceStatus.Paid,
            lastEvent: SeedNow.AddDays(-2));

        EnsureBillingPayment(dbContext, PayBlueHarbor1Id,
            invoiceId: InvBlueHarbor1Id,
            tenantId: BlueHarborId,
            externalPaymentId: "pi_3RBHLPay2026Apr",
            method: "card",
            amountMinor: 29900,
            currency: "CAD",
            status: BillingPaymentStatus.Succeeded,
            paidAt: SeedNow.AddDays(-2),
            lastEvent: SeedNow.AddDays(-2));

        EnsureTenantEntitlementSnapshot(dbContext, EntBlueHarborId,
            tenantId: BlueHarborId,
            planCode: "business-monthly",
            subscriptionStatus: TenantSubscriptionStatus.Active,
            isActive: true,
            isInGracePeriod: false,
            effectiveUntil: SeedNow.AddDays(28),
            featureJson: """{"maxSpaces":200,"maxQuestionsPerSpace":1000,"aiGeneration":true,"analytics":true,"prioritySupport":true}""");
    }

    // -------------------------------------------------------------------------
    // Webhook inbox scenarios: 5 rows covering all status variants.
    // -------------------------------------------------------------------------

    private static void SeedWebhookInboxScenarios(TenantDbContext dbContext)
    {
        // 1) Processed — checkout.session.completed (BlueHarbor onboarding)
        EnsureWebhookInbox(dbContext, Wh1Id,
            tenantId: BlueHarborId,
            externalEventId: "evt_1RBHLWebCheckout",
            eventType: "checkout.session.completed",
            status: ControlPlaneMessageStatus.Completed,
            attemptCount: 1,
            receivedDate: SeedNow.AddDays(-2),
            eventCreatedAt: SeedNow.AddDays(-2),
            processedDate: SeedNow.AddDays(-2),
            payloadJson: """{"id":"evt_1RBHLWebCheckout","type":"checkout.session.completed","data":{"object":{"id":"cs_1RBHLCheckout","customer":"cus_Q8fBHL1xY2ZaBcDe","subscription":"sub_1RBHLAbCdEfGhIjKlMn","status":"complete","currency":"cad","amount_total":29900}}}""");

        // 2) Processed — customer.subscription.updated (NorthPeak renewal)
        EnsureWebhookInbox(dbContext, Wh2Id,
            tenantId: NorthPeakTenantId,
            externalEventId: "evt_1RNPWebSubUpdated",
            eventType: "customer.subscription.updated",
            status: ControlPlaneMessageStatus.Completed,
            attemptCount: 1,
            receivedDate: SeedNow.AddDays(-9),
            eventCreatedAt: SeedNow.AddDays(-9),
            processedDate: SeedNow.AddDays(-9),
            payloadJson: """{"id":"evt_1RNPWebSubUpdated","type":"customer.subscription.updated","data":{"object":{"id":"sub_1RNPAbCdEfGhIjKlMnOp","customer":"cus_Q8fNPA1xY2ZaBcDe","status":"active","current_period_start":1744156800,"current_period_end":1746748800}}}""");

        // 3) Failed — invoice.payment_failed (MapleForge card declined, with error)
        EnsureWebhookInbox(dbContext, Wh3Id,
            tenantId: MapleForgeId,
            externalEventId: "evt_1RMFMWebPayFailed",
            eventType: "invoice.payment_failed",
            status: ControlPlaneMessageStatus.Failed,
            attemptCount: 3,
            receivedDate: SeedNow.AddDays(-5),
            eventCreatedAt: SeedNow.AddDays(-5),
            lastAttemptDate: SeedNow.AddDays(-4),
            lastError: "A Stripe webhook delivery failed validation and has been queued for manual review. StripeException: No such payment_intent: 'pi_3RMFMPay2026Apr'. Verify that the Stripe secret key and webhook signing secret match the environment.",
            payloadJson: """{"id":"evt_1RMFMWebPayFailed","type":"invoice.payment_failed","data":{"object":{"id":"in_1RMFMInvApr2026","customer":"cus_Q8fMFM1xY2ZaBcDe","subscription":"sub_1RMFMAbCdEfGhIjKlMn","status":"open","amount_due":9900,"currency":"cad","attempt_count":2}}}""");

        // 4) Pending — invoice.paid (NorthPeak, queued for processing)
        EnsureWebhookInbox(dbContext, Wh4Id,
            tenantId: NorthPeakTenantId,
            externalEventId: "evt_1RNPWebInvPaid",
            eventType: "invoice.paid",
            status: ControlPlaneMessageStatus.Pending,
            attemptCount: 0,
            receivedDate: SeedNow,
            eventCreatedAt: SeedNow,
            nextAttemptDate: SeedNow.AddMinutes(30),
            payloadJson: """{"id":"evt_1RNPWebInvPaid","type":"invoice.paid","data":{"object":{"id":"in_1RNPInvApr2026","customer":"cus_Q8fNPA1xY2ZaBcDe","subscription":"sub_1RNPAbCdEfGhIjKlMnOp","status":"paid","amount_paid":9900,"currency":"cad"}}}""");

        // 5) Processed — customer.subscription.deleted (Aurora cancellation)
        EnsureWebhookInbox(dbContext, Wh5Id,
            tenantId: AuroraClinicId,
            externalEventId: "evt_1RACSWebSubDeleted",
            eventType: "customer.subscription.deleted",
            status: ControlPlaneMessageStatus.Completed,
            attemptCount: 1,
            receivedDate: SeedNow.AddDays(-9),
            eventCreatedAt: SeedNow.AddDays(-9),
            processedDate: SeedNow.AddDays(-9),
            payloadJson: """{"id":"evt_1RACSWebSubDeleted","type":"customer.subscription.deleted","data":{"object":{"id":"sub_1RACSAbCdEfGhIjKlMn","customer":"cus_Q8fACS1xY2ZaBcDe","status":"canceled","canceled_at":1744243200}}}""");
    }

    // -------------------------------------------------------------------------
    // Email outbox scenarios: Pending, Completed, Failed.
    // -------------------------------------------------------------------------

    private static void SeedEmailOutboxScenarios(TenantDbContext dbContext)
    {
        // 1) Pending — invoice receipt for BlueHarbor
        EnsureEmailOutbox(dbContext, Email1Id,
            recipientEmail: "it@blueharborlegal.com",
            subject: "Your Querify invoice receipt — April 2026",
            htmlBody: "<p>Hi BlueHarbor Legal,</p><p>Your Querify Business monthly subscription renewed successfully. Invoice amount: CAD $299.00. Thank you for your continued trust in Querify.</p>",
            textBody: "Your Querify Business monthly subscription renewed successfully. Invoice amount: CAD $299.00.",
            fromEmail: "billing@querify.net",
            fromName: "Querify Billing",
            status: ControlPlaneMessageStatus.Pending,
            queuedDate: SeedNow.AddDays(-2),
            nextAttemptDate: SeedNow.AddMinutes(15));

        // 2) Completed — payment failure notification for MapleForge
        EnsureEmailOutbox(dbContext, Email2Id,
            recipientEmail: "finance@mapleforge.io",
            subject: "Payment issue on your Querify subscription",
            htmlBody: "<p>Hi MapleForge Media,</p><p>We were unable to process your latest Querify subscription payment. Please update your payment method to avoid interruption to your service. Your account remains active during the grace period ending April 17, 2026.</p>",
            textBody: "We were unable to process your latest Querify subscription payment. Please update your payment method to avoid interruption.",
            fromEmail: "billing@querify.net",
            fromName: "Querify Billing",
            status: ControlPlaneMessageStatus.Completed,
            queuedDate: SeedNow.AddDays(-5),
            processedDate: SeedNow.AddDays(-5));

        // 3) Failed retryable — payment method reminder for PacificTrail
        EnsureEmailOutbox(dbContext, Email3Id,
            recipientEmail: "ops@pacifictrailstudio.com",
            subject: "Action required: add a payment method before your Querify trial ends",
            htmlBody: "<p>Hi Pacific Trail Studio,</p><p>Your Querify trial is active and will end on May 15, 2026. To avoid losing access, please add a payment method to continue on the Starter plan at CAD $29.00/month.</p>",
            textBody: "Your Querify trial is active and will end on May 15, 2026. Please add a payment method to continue.",
            fromEmail: "billing@querify.net",
            fromName: "Querify Billing",
            status: ControlPlaneMessageStatus.Failed,
            queuedDate: SeedNow.AddDays(-3),
            attemptCount: 2,
            lastAttemptDate: SeedNow.AddDays(-1),
            nextAttemptDate: SeedNow.AddHours(6),
            lastError: "SMTP delivery failed: Connection refused (host.docker.internal:1025). Delivery will be retried automatically.");
    }

    // -------------------------------------------------------------------------
    // Ensure helpers
    // -------------------------------------------------------------------------

    private static Tenant EnsureTenant(
        TenantDbContext dbContext,
        Guid id,
        string slug,
        string name,
        TenantEdition edition,
        string productConnectionString)
    {
        var tenant = dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefault(t => t.Id == id);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = id,
                Slug = slug,
                Name = name,
                Edition = edition,
                Module = ModuleEnum.QnA,
                ConnectionString = productConnectionString,
                IsActive = true
            };
            dbContext.Tenants.Add(tenant);
        }
        else
        {
            RestoreEntity(tenant);
            tenant.Slug = slug;
            tenant.Name = name;
            tenant.Edition = edition;
            tenant.Module = ModuleEnum.QnA;
            tenant.ConnectionString = productConnectionString;
            tenant.IsActive = true;
        }

        return tenant;
    }

    private static bool HasSeedTenantBillingData(TenantDbContext dbContext, Guid seedTenantId)
    {
        return dbContext.BillingCustomers
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == BcSeedTenantId && entry.TenantId == seedTenantId) &&
               dbContext.TenantSubscriptions
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == SubSeedTenantId && entry.TenantId == seedTenantId) &&
               dbContext.BillingProviderSubscriptions
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == PsSeedTenantId && entry.TenantId == seedTenantId) &&
               dbContext.BillingInvoices
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Count(entry =>
                       entry.TenantId == seedTenantId &&
                       (entry.Id == InvSeedTenant1Id || entry.Id == InvSeedTenant2Id)) == 2 &&
               dbContext.BillingPayments
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Count(entry =>
                       entry.TenantId == seedTenantId &&
                       (entry.Id == PaySeedTenant1Id || entry.Id == PaySeedTenant2Id)) == 2 &&
               dbContext.TenantEntitlementSnapshots
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == EntSeedTenantId && entry.TenantId == seedTenantId);
    }

    private static bool HasScenarioBillingData(TenantDbContext dbContext)
    {
        return dbContext.Tenants
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == NorthPeakTenantId) &&
               dbContext.BillingWebhookInboxes
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == Wh1Id) &&
               dbContext.EmailOutboxes
                   .IgnoreQueryFilters()
                   .AsNoTracking()
                   .Any(entry => entry.Id == Email1Id);
    }

    private static void EnsureBillingCustomer(
        TenantDbContext dbContext,
        Guid id,
        Guid tenantId,
        string externalId,
        string email,
        string countryCode,
        DateTime lastEvent)
    {
        var entity = dbContext.BillingCustomers
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new BillingCustomer
            {
                Id = id,
                TenantId = tenantId,
                Provider = BillingProviderType.Stripe,
                ExternalCustomerId = externalId,
                Email = email,
                CountryCode = countryCode,
                LastEventCreatedAtUtc = lastEvent
            };
            dbContext.BillingCustomers.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantId = tenantId;
            entity.Provider = BillingProviderType.Stripe;
            entity.ExternalCustomerId = externalId;
            entity.Email = email;
            entity.CountryCode = countryCode;
            entity.LastEventCreatedAtUtc = lastEvent;
        }
    }

    private static void EnsureTenantSubscription(
        TenantDbContext dbContext,
        Guid id,
        Guid tenantId,
        string planCode,
        BillingIntervalType interval,
        TenantSubscriptionStatus status,
        string currency,
        string countryCode,
        DateTime periodStart,
        DateTime periodEnd,
        DateTime? trialEndsAt = null,
        DateTime? graceUntil = null,
        DateTime? cancelledAt = null,
        DateTime? lastEvent = null)
    {
        var entity = dbContext.TenantSubscriptions
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new TenantSubscription
            {
                Id = id,
                TenantId = tenantId,
                PlanCode = planCode,
                BillingInterval = interval,
                Status = status,
                Currency = currency,
                CountryCode = countryCode,
                CurrentPeriodStartUtc = periodStart,
                CurrentPeriodEndUtc = periodEnd,
                TrialEndsAtUtc = trialEndsAt,
                GraceUntilUtc = graceUntil,
                CancelledAtUtc = cancelledAt,
                DefaultProvider = BillingProviderType.Stripe,
                LastEventCreatedAtUtc = lastEvent
            };
            dbContext.TenantSubscriptions.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantId = tenantId;
            entity.PlanCode = planCode;
            entity.BillingInterval = interval;
            entity.Status = status;
            entity.Currency = currency;
            entity.CountryCode = countryCode;
            entity.CurrentPeriodStartUtc = periodStart;
            entity.CurrentPeriodEndUtc = periodEnd;
            entity.TrialEndsAtUtc = trialEndsAt;
            entity.GraceUntilUtc = graceUntil;
            entity.CancelledAtUtc = cancelledAt;
            entity.DefaultProvider = BillingProviderType.Stripe;
            entity.LastEventCreatedAtUtc = lastEvent;
        }
    }

    private static void EnsureBillingProviderSubscription(
        TenantDbContext dbContext,
        Guid id,
        Guid tenantSubscriptionId,
        Guid tenantId,
        string externalSubId,
        string externalPriceId,
        string externalProductId,
        TenantSubscriptionStatus status,
        DateTime periodStart,
        DateTime periodEnd,
        DateTime? trialEndsAt = null,
        DateTime? cancelledAt = null,
        DateTime? lastEvent = null)
    {
        var entity = dbContext.BillingProviderSubscriptions
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new BillingProviderSubscription
            {
                Id = id,
                TenantSubscriptionId = tenantSubscriptionId,
                TenantId = tenantId,
                Provider = BillingProviderType.Stripe,
                ExternalSubscriptionId = externalSubId,
                ExternalPriceId = externalPriceId,
                ExternalProductId = externalProductId,
                Status = status,
                CurrentPeriodStartUtc = periodStart,
                CurrentPeriodEndUtc = periodEnd,
                TrialEndsAtUtc = trialEndsAt,
                CancelledAtUtc = cancelledAt,
                LastEventCreatedAtUtc = lastEvent
            };
            dbContext.BillingProviderSubscriptions.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantSubscriptionId = tenantSubscriptionId;
            entity.TenantId = tenantId;
            entity.Provider = BillingProviderType.Stripe;
            entity.ExternalSubscriptionId = externalSubId;
            entity.ExternalPriceId = externalPriceId;
            entity.ExternalProductId = externalProductId;
            entity.Status = status;
            entity.CurrentPeriodStartUtc = periodStart;
            entity.CurrentPeriodEndUtc = periodEnd;
            entity.TrialEndsAtUtc = trialEndsAt;
            entity.CancelledAtUtc = cancelledAt;
            entity.LastEventCreatedAtUtc = lastEvent;
        }
    }

    private static void EnsureBillingInvoice(
        TenantDbContext dbContext,
        Guid id,
        Guid tenantSubscriptionId,
        Guid tenantId,
        string externalInvoiceId,
        long amountMinor,
        string currency,
        DateTime? dueDate,
        DateTime? paidAt,
        BillingInvoiceStatus status,
        DateTime? lastEvent)
    {
        var entity = dbContext.BillingInvoices
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new BillingInvoice
            {
                Id = id,
                TenantSubscriptionId = tenantSubscriptionId,
                TenantId = tenantId,
                Provider = BillingProviderType.Stripe,
                ExternalInvoiceId = externalInvoiceId,
                AmountMinor = amountMinor,
                Currency = currency,
                DueDateUtc = dueDate,
                PaidAtUtc = paidAt,
                Status = status,
                LastEventCreatedAtUtc = lastEvent
            };
            dbContext.BillingInvoices.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantSubscriptionId = tenantSubscriptionId;
            entity.TenantId = tenantId;
            entity.Provider = BillingProviderType.Stripe;
            entity.ExternalInvoiceId = externalInvoiceId;
            entity.AmountMinor = amountMinor;
            entity.Currency = currency;
            entity.DueDateUtc = dueDate;
            entity.PaidAtUtc = paidAt;
            entity.Status = status;
            entity.LastEventCreatedAtUtc = lastEvent;
        }
    }

    private static void EnsureBillingPayment(
        TenantDbContext dbContext,
        Guid id,
        Guid invoiceId,
        Guid tenantId,
        string externalPaymentId,
        string method,
        long amountMinor,
        string currency,
        BillingPaymentStatus status,
        DateTime? paidAt,
        DateTime? lastEvent,
        string? failureCode = null,
        string? failureMessage = null)
    {
        var entity = dbContext.BillingPayments
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new BillingPayment
            {
                Id = id,
                BillingInvoiceId = invoiceId,
                TenantId = tenantId,
                Provider = BillingProviderType.Stripe,
                ExternalPaymentId = externalPaymentId,
                Method = method,
                AmountMinor = amountMinor,
                Currency = currency,
                Status = status,
                PaidAtUtc = paidAt,
                FailureCode = failureCode,
                FailureMessage = failureMessage,
                LastEventCreatedAtUtc = lastEvent
            };
            dbContext.BillingPayments.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.BillingInvoiceId = invoiceId;
            entity.TenantId = tenantId;
            entity.Provider = BillingProviderType.Stripe;
            entity.ExternalPaymentId = externalPaymentId;
            entity.Method = method;
            entity.AmountMinor = amountMinor;
            entity.Currency = currency;
            entity.Status = status;
            entity.PaidAtUtc = paidAt;
            entity.FailureCode = failureCode;
            entity.FailureMessage = failureMessage;
            entity.LastEventCreatedAtUtc = lastEvent;
        }
    }

    private static void EnsureTenantEntitlementSnapshot(
        TenantDbContext dbContext,
        Guid id,
        Guid tenantId,
        string planCode,
        TenantSubscriptionStatus subscriptionStatus,
        bool isActive,
        bool isInGracePeriod,
        DateTime? effectiveUntil,
        string featureJson)
    {
        var entity = dbContext.TenantEntitlementSnapshots
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new TenantEntitlementSnapshot
            {
                Id = id,
                TenantId = tenantId,
                PlanCode = planCode,
                SubscriptionStatus = subscriptionStatus,
                IsActive = isActive,
                IsInGracePeriod = isInGracePeriod,
                EffectiveUntilUtc = effectiveUntil,
                FeatureJson = featureJson
            };
            dbContext.TenantEntitlementSnapshots.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantId = tenantId;
            entity.PlanCode = planCode;
            entity.SubscriptionStatus = subscriptionStatus;
            entity.IsActive = isActive;
            entity.IsInGracePeriod = isInGracePeriod;
            entity.EffectiveUntilUtc = effectiveUntil;
            entity.FeatureJson = featureJson;
        }
    }

    private static void EnsureWebhookInbox(
        TenantDbContext dbContext,
        Guid id,
        Guid? tenantId,
        string externalEventId,
        string eventType,
        ControlPlaneMessageStatus status,
        int attemptCount,
        DateTime receivedDate,
        DateTime eventCreatedAt,
        DateTime? processedDate = null,
        DateTime? lastAttemptDate = null,
        DateTime? nextAttemptDate = null,
        string? lastError = null,
        string payloadJson = "{}")
    {
        var entity = dbContext.BillingWebhookInboxes
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new BillingWebhookInbox
            {
                Id = id,
                TenantId = tenantId,
                Provider = BillingProviderType.Stripe,
                ExternalEventId = externalEventId,
                EventType = eventType,
                PayloadJson = payloadJson,
                SignatureValid = true,
                IsLiveMode = false,
                ProviderAccountId = "acct_1RQuerifyStripe",
                ReceivedDateUtc = receivedDate,
                EventCreatedAtUtc = eventCreatedAt,
                Status = status,
                AttemptCount = attemptCount,
                ProcessedDateUtc = processedDate,
                LastAttemptDateUtc = lastAttemptDate,
                NextAttemptDateUtc = nextAttemptDate,
                LastError = lastError
            };
            dbContext.BillingWebhookInboxes.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.TenantId = tenantId;
            entity.Provider = BillingProviderType.Stripe;
            entity.ExternalEventId = externalEventId;
            entity.EventType = eventType;
            entity.PayloadJson = payloadJson;
            entity.SignatureValid = true;
            entity.IsLiveMode = false;
            entity.ProviderAccountId = "acct_1RQuerifyStripe";
            entity.ReceivedDateUtc = receivedDate;
            entity.EventCreatedAtUtc = eventCreatedAt;
            entity.Status = status;
            entity.AttemptCount = attemptCount;
            entity.ProcessedDateUtc = processedDate;
            entity.LastAttemptDateUtc = lastAttemptDate;
            entity.NextAttemptDateUtc = nextAttemptDate;
            entity.LastError = lastError;
        }
    }

    private static void EnsureEmailOutbox(
        TenantDbContext dbContext,
        Guid id,
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody,
        string fromEmail,
        string fromName,
        ControlPlaneMessageStatus status,
        DateTime queuedDate,
        DateTime? processedDate = null,
        int attemptCount = 0,
        DateTime? lastAttemptDate = null,
        DateTime? nextAttemptDate = null,
        string? lastError = null)
    {
        var entity = dbContext.EmailOutboxes
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null)
        {
            entity = new EmailOutbox
            {
                Id = id,
                RecipientEmail = recipientEmail,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody,
                FromEmail = fromEmail,
                FromName = fromName,
                QueuedDateUtc = queuedDate,
                Status = status,
                AttemptCount = attemptCount,
                ProcessedDateUtc = processedDate,
                LastAttemptDateUtc = lastAttemptDate,
                NextAttemptDateUtc = nextAttemptDate,
                LastError = lastError
            };
            dbContext.EmailOutboxes.Add(entity);
        }
        else
        {
            RestoreEntity(entity);
            entity.RecipientEmail = recipientEmail;
            entity.Subject = subject;
            entity.HtmlBody = htmlBody;
            entity.TextBody = textBody;
            entity.FromEmail = fromEmail;
            entity.FromName = fromName;
            entity.QueuedDateUtc = queuedDate;
            entity.Status = status;
            entity.AttemptCount = attemptCount;
            entity.ProcessedDateUtc = processedDate;
            entity.LastAttemptDateUtc = lastAttemptDate;
            entity.NextAttemptDateUtc = nextAttemptDate;
            entity.LastError = lastError;
        }
    }

    private static void RestoreEntity(BaseEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedBy = null;
        entity.DeletedDate = null;
    }
}
