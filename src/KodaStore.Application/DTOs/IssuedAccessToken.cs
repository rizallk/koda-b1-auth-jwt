namespace KodaStore.Application.DTOs;

public sealed record IssuedAccessToken(
  string AccessToken,
  DateTimeOffset ExpiredAtUtc
);