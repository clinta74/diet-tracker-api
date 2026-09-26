#nullable enable
using diet_tracker_api.DataLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace diet_tracker_api.Services;

public interface IPasswordService
{
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a stored hash. Pass a null hash when the account does not
    /// exist so the response takes as long as a real check and does not reveal which emails exist.
    /// </summary>
    PasswordVerificationResult Verify(string? passwordHash, string password);
}

/// <summary>
/// Hashes passwords with ASP.NET Core Identity's PBKDF2 hasher. Hashes created with BCrypt before
/// the switch still verify and report <see cref="PasswordVerificationResult.SuccessRehashNeeded"/>
/// so they are upgraded on the next successful login.
/// </summary>
public class PasswordService(IPasswordHasher<UserCredentials> hasher) : IPasswordService
{
    private const string LegacyBcryptPrefix = "$2";

    private readonly IPasswordHasher<UserCredentials> _hasher = hasher;
    private readonly string _dummyHash = hasher.HashPassword(null!, Guid.NewGuid().ToString());

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public PasswordVerificationResult Verify(string? passwordHash, string password)
    {
        if (passwordHash == null)
        {
            _hasher.VerifyHashedPassword(null!, _dummyHash, password);
            return PasswordVerificationResult.Failed;
        }

        if (passwordHash.StartsWith(LegacyBcryptPrefix, StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }

        return _hasher.VerifyHashedPassword(null!, passwordHash, password);
    }
}
