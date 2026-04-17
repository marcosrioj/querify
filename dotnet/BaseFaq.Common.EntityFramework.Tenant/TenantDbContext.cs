using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Security;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.User.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BaseFaq.Common.EntityFramework.Tenant;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options,
    ISessionService sessionService,
    IConfiguration configuration,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IHttpContextAccessor httpContextAccessor)
    : BaseDbContext<TenantDbContext>(
        options,
        sessionService,
        configuration,
        tenantConnectionStringProvider,
        httpContextAccessor)
{
    public DbSet<Entities.Tenant> Tenants { get; set; } = null!;
    public DbSet<BillingCustomer> BillingCustomers { get; set; } = null!;
    public DbSet<BillingInvoice> BillingInvoices { get; set; } = null!;
    public DbSet<BillingPayment> BillingPayments { get; set; } = null!;
    public DbSet<BillingProviderSubscription> BillingProviderSubscriptions { get; set; } = null!;
    public DbSet<BillingWebhookInbox> BillingWebhookInboxes { get; set; } = null!;
    public DbSet<EmailOutbox> EmailOutboxes { get; set; } = null!;
    public DbSet<TenantConnection> TenantConnections { get; set; } = null!;
    public DbSet<TenantEntitlementSnapshot> TenantEntitlementSnapshots { get; set; } = null!;
    public DbSet<TenantSubscription> TenantSubscriptions { get; set; } = null!;
    public DbSet<TenantUser> TenantUsers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Common.EntityFramework.Tenant.Configurations"
    ];

    protected override bool UseTenantConnectionString => false;
    protected override AppEnum SessionApp => AppEnum.Tenant;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var converter = new ValueConverter<string, string>(
            value => EncryptConnectionString(value),
            value => DecryptConnectionString(value));

        modelBuilder.Entity<Entities.Tenant>()
            .Property(tenant => tenant.ConnectionString)
            .HasConversion(converter);

        modelBuilder.Entity<TenantConnection>()
            .Property(connection => connection.ConnectionString)
            .HasConversion(converter);
    }

    public async Task<TenantConnection> GetCurrentTenantConnection(AppEnum app,
        CancellationToken cancellationToken = default)
    {
        var connection = await TenantConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.App == app && item.IsCurrent,
                cancellationToken);

        if (connection is null)
        {
            throw new ApiErrorException(
                $"Current tenant connection for {app} was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return connection;
    }

    public async Task<string> GetTenantConnectionString(Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == tenantId && item.IsActive,
                cancellationToken);

        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (string.IsNullOrWhiteSpace(tenant.ConnectionString))
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' has an invalid connection string.",
                errorCode: (int)HttpStatusCode.InternalServerError);
        }

        return tenant.ConnectionString;
    }

    public async Task<Guid> GetUserId(string externalUserId, CancellationToken cancellationToken = default)
    {
        var user = await Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.ExternalId == externalUserId,
                cancellationToken);

        if (user is null)
        {
            throw new ApiErrorException(
                $"User with external id '{externalUserId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return user.Id;
    }

    public async Task<Guid> EnsureUser(string externalUserId, string? userName, string? email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalUserId) || string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email))
        {
            throw new ApiErrorException(
                $"User: '{externalUserId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var user = await Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.ExternalId == externalUserId,
                cancellationToken);

        if (user is not null)
        {
            return user.Id;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            GivenName = userName,
            ExternalId = externalUserId,
            Email = email,
            Role = UserRoleType.Member
        };

        await Users.AddAsync(user, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return user.Id;
    }

    private static string EncryptConnectionString(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || IsEncrypted(value))
        {
            return value;
        }

        return StringCipher.Instance.Encrypt(value) ?? string.Empty;
    }

    private static string DecryptConnectionString(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsEncrypted(value))
        {
            return value;
        }

        return StringCipher.Instance.Decrypt(value) ?? string.Empty;
    }

    private static bool IsEncrypted(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.StartsWith("v1:", StringComparison.Ordinal);
    }

}
