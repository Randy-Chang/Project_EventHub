using EventHub.Domain.Participants;

namespace EventHub.Domain.Tests;

public sealed class ParticipantTests
{
    [Fact]
    public void Create_NormalizesIdentityAndUsesNicknameForDisplay()
    {
        var participant = Participant.Create(
            Guid.NewGuid(),
            " 王小明 ",
            " 小明 ",
            " emp-001 ",
            " 研發部 ",
            " 8 ",
            "credential-hash",
            DateTimeOffset.UtcNow);

        Assert.Equal("王小明", participant.Name);
        Assert.Equal("小明", participant.DisplayName);
        Assert.Equal("EMP-001", participant.NormalizedEmployeeNumber);
        Assert.Equal("研發部", participant.Department);
    }
}
