using System.ComponentModel.DataAnnotations;

namespace BaseFaq.Tenant.Worker.Business.Email.Options;

public sealed class EmailDeliveryOptions
{
    public const string SectionName = "TenantWorker:EmailDelivery";

    [Required]
    public string Provider { get; set; } = "smtp";

    [Required]
    [EmailAddress]
    public string DefaultFromAddress { get; set; } = "noreply@basefaq.local";

    [Required]
    public string DefaultFromName { get; set; } = "BaseFaq Local";

    public SmtpDeliveryOptions Smtp { get; set; } = new();
}

public sealed class SmtpDeliveryOptions
{
    [Required]
    public string Host { get; set; } = "host.docker.internal";

    [Range(1, 65535)]
    public int Port { get; set; } = 1025;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; }
}
