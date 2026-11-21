using KodaStore.Application.DTOs;
using KodaStore.Application.DTOs.Auth;

namespace KodaStore.Application.Services;

public interface IJwtTokenService
{
  IssuedAccessToken IssueAccessToken(AuthUserResponse user);
}