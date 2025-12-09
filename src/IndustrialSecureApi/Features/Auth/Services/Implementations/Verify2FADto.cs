namespace IndustrialSecureApi.Features.Auth;

public record Verify2FADto(
    string Username,
    string Code
);