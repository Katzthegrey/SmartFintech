namespace SmartGuardFinancial.Modules.Identity.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");

public record RefreshTokenRequest(string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);