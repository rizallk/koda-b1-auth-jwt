namespace KodaStore.Application.DTOs.Auth;

public sealed record LoginResults(
  bool Succeeded,
  AuthUserResponse? User
);