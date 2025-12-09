using OtpNet;
using QRCoder;

namespace IndustrialSecureApi.Features.Auth;

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        // Genera una chiave segreta Base32 di 20 byte (160 bit)
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public bool ValidateCode(string secret, string code)
    {
        try
        {
            // Decodifica la chiave segreta
            var keyBytes = Base32Encoding.ToBytes(secret);

            // Crea un TOTP con periodo di 30 secondi
            var totp = new Totp(keyBytes);

            // Valida il codice (tolleranza di 1 periodo = 30 secondi)
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }

    public string GetQrCodeUri(string email, string secret, string issuer = "Industrial Secure API")
    {
        // Crea l'URI per il QR code (formato standard per Google Authenticator)
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
        return uri;
    }
}