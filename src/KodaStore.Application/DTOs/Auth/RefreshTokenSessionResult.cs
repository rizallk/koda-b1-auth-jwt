namespace KodaStore.Application.DTOs.Auth;

public sealed record RefreshTokenSessionResult(
  AuthUserResponse User,
  IssuedAccessToken AccessToken,
  string RawRefreshToken,
  DateTimeOffset RefreshExpiredAtUtc
);