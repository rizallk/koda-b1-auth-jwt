namespace KodaStore.Application.DTOs.Auth;

public sealed record LoginResponse(
  string Message,
  AuthUserResponse User,
  string AccessToken,
  string TokenType,
  DateTimeOffset ExpiresAtUtc
);