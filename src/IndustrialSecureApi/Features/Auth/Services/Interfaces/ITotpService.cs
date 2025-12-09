namespace IndustrialSecureApi.Features.Auth.Services.Interfaces;

public interface ITotpService
{
    string GenerateSecret();
    bool ValidateCode(string secret, string code);
    string GetQrCodeUri(string email, string secret, string issuer = "Industrial Secure API");
}