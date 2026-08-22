namespace EventHub.Application.Abstractions;

public interface ICredentialService
{
    string GenerateToken();

    string HashToken(string token);

    bool Matches(string token, string expectedHash);
}
