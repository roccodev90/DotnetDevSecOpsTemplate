namespace IndustrialSecureApi.Infrastructure;

public record AuditEntry(
    Guid Id,
    string User,
    string Action,
    string Entity,
    DateTime When,
    string OldValues,
    string NewValues
);