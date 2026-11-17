using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Identity;

public sealed class IdentityAuthService(UserManager<AppUser> user) : IAuthService
{
  private readonly UserManager<AppUser> _user = user;

  public async Task<LoginResults> LoginAsync(LoginRequest request, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    var email = request.Email.Trim();
    var user = await _user.FindByEmailAsync(email);

    if (user is null)
    {
      return new LoginResults(false, null);
    }

    var passwordValid = await _user.CheckPasswordAsync(user, request.Password);

    if (!passwordValid)
    {
      return new LoginResults(false, null);
    }

    var response = new AuthUserResponse(
        user.Id, user.Email!, user.DisplayName
    );
    return new LoginResults(true, response);
  }

  public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    var email = request.Email.Trim();
    var existingUser = await _user.FindByEmailAsync(email);
    if (existingUser is not null)
    {
      return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email sudah terdaftar"]);
    }
    var user = new AppUser
    {
      Id = Guid.NewGuid(),
      UserName = email,
      Email = email,
      DisplayName = request.DisplayName.Trim(),
      CreatedAtUtc = DateTimeOffset.UtcNow
    };

    IdentityResult identityResult;

    try
    {
      identityResult = await _user.CreateAsync(user, request.Password);
    }
    catch (DbUpdateException e)
        when (e.InnerException is SqlException sqlError
        && sqlError.Number is 2601 or 2627)
    {
      return new RegisterResult(RegisterStatus.DuplicateEmail, null, [
          "Email sudah terdaftar"
      ]);
    }

    if (!identityResult.Succeeded)
    {
      var error = identityResult.Errors.ToArray();

      var isDuplicate = error.Any(error => error.Code is "DuplicateEmail" or "DuplicateUsername");

      if (isDuplicate)
      {
        return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email sudah terdaftar"]);
      }
      return new RegisterResult(RegisterStatus.ValidationError,
      null,
      error.Select(error => error.Description).ToArray());

    }
    var response = new AuthUserResponse(
        user.Id,
        user.Email!,
        user.DisplayName
    );

    return new RegisterResult(RegisterStatus.Created, response, []);
  }
}