using Microsoft.AspNetCore.Identity;

namespace IndustrialSecureApi.Infrastructure;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? TotpSecret { get; set; }  // Chiave segreta TOTP
    public bool TotpEnabled { get; set; }    // Flag se 2FA è abilitato
}