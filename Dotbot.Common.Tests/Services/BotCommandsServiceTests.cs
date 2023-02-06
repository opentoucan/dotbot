using Moq.AutoMock;

namespace Dotbot.Common.Tests.Services;

public class BotCommandsServiceTests
{
    [Fact]
    public async Task ShouldSearchCommands()
    {
        var mocker = new AutoMocker();

        const string serverId = "abc123";

        mocker.GetMock<IBotCommandRepository>()
            .Setup(x => x.GetAllNames(serverId))
            .ReturnsAsync(Ok(new List<string> { "apple", "orange", "test", "test2" }));

        var sut = mocker.CreateInstance<BotCommandsService>();

        var (isSuccess, _, value) = await sut.Search(serverId, "test");

        isSuccess.Should().Be(true);

        value.Should().NotBeEmpty();

        mocker.GetMock<IBotCommandRepository>()
            .Verify(x => x.GetAllNames(serverId), Times.Once);
    }

    [Fact]
    public async Task ShouldFailOnNoCommands()
    {
        var mocker = new AutoMocker();

        const string serverId = "abc123";

        mocker.GetMock<IBotCommandRepository>()
            .Setup(x => x.GetAllNames(serverId))
            .ReturnsAsync(Ok(new List<string>()));

        var sut = mocker.CreateInstance<BotCommandsService>();

        var (_, isFail, value) = await sut.Search(serverId, "test");

        isFail.Should().Be(true);

        value.Should().BeNull();

        mocker.GetMock<IBotCommandRepository>()
            .Verify(x => x.GetAllNames(serverId), Times.Once);
    }
}