namespace KodaStore.Application.DTOs.Auth;

public sealed record AuthUserResponse(
  Guid Id,
  string Email,
  string DisplayName
);