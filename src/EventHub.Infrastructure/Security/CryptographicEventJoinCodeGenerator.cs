using System.Security.Cryptography;
using EventHub.Application.Abstractions;
using EventHub.Domain.Events;

namespace EventHub.Infrastructure.Security;

public sealed class CryptographicEventJoinCodeGenerator : IEventJoinCodeGenerator
{
    public string Generate()
    {
        Span<char> characters = stackalloc char[EventJoinCode.Length];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = EventJoinCode.AllowedCharacters[
                RandomNumberGenerator.GetInt32(EventJoinCode.AllowedCharacters.Length)];
        }

        return new string(characters);
    }
}
