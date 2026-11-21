namespace KodaStore.Application.DTOs.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email
);