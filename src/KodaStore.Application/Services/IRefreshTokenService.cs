using System;
using KodaStore.Application.DTOs.Auth;

namespace KodaStore.Application.Services;

public interface IRefreshTokenService
{
  Task<RefreshTokenSessionResult> CreateSessionAsync(AuthUserResponse user, CancellationToken ct = default);
  Task<RefreshTokenSessionResult> RotateAsync(string rawRefreshToken, CancellationToken ct = default);
  Task RevokeFamilyAsync(string rawRefreshToken, CancellationToken ct = default);

}