namespace KodaStore.Application.DTOs.Auth;

public enum RegisterStatus
{
  Created, DuplicateEmail, ValidationError
}

public sealed record RegisterResult(
  RegisterStatus Status,
  AuthUserResponse? User,
  string[] Error
);