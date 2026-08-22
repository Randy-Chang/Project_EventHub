using System.Security.Cryptography;
using System.Text;
using EventHub.Application.Abstractions;

namespace EventHub.Infrastructure.Security;

public sealed class CryptographicCredentialService : ICredentialService
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public bool Matches(string token, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var actualHash = Encoding.ASCII.GetBytes(HashToken(token));
        var expectedHashBytes = Encoding.ASCII.GetBytes(expectedHash);
        return actualHash.Length == expectedHashBytes.Length &&
            CryptographicOperations.FixedTimeEquals(actualHash, expectedHashBytes);
    }
}
