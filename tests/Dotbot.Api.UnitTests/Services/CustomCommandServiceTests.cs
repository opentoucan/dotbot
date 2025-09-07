using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Dotbot.Api.Application.Queries;
using Dotbot.Api.Queries;
using Dotbot.Api.Services;
using Dotbot.Api.Settings;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.JsonModels;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace Dotbot.Api.UnitTests.Services;

public class CustomCommandServiceTests
{
    //Mocks
    private static readonly IAmazonS3 AmazonS3ClientMock = Substitute.For<IAmazonS3>();
    private static readonly ILogger<CustomCommandService> LoggerMock = Substitute.For<ILogger<CustomCommandService>>();
    private static readonly IGuildRepository GuildRepositoryMock = Substitute.For<IGuildRepository>();

    //Fakes
    private static readonly IFileUploadService FileUploadService =
        new FileUploadService(AmazonS3ClientMock, Substitute.For<ILogger<FileUploadService>>());

    private static readonly IOptions<DiscordSettings> DiscordSettings =
        Options.Create(new DiscordSettings { BucketEnvPrefix = "test", Token = "some_token" });

    private readonly MockHttpMessageHandler _handler = new();
    private DotbotContext _dbContext = null!;
    private IGuildQueries _guildQueries = null!;
    private IGuildRepository _guildRepository = null!;
    private HttpClient _httpClient = null!;

    [Before(Test)]
    public async Task Before()
    {
        _httpClient = new HttpClient(_handler);
        _dbContext = new DotbotContext(
            new DbContextOptionsBuilder<DotbotContext>()
                .UseInMemoryDatabase(TestContext.Current!.TestDetails.TestId)
                .Options);
        _guildQueries = new GuildQueries(_dbContext);
        _guildRepository = new GuildRepository(_dbContext);
        await _dbContext.Database.EnsureCreatedAsync();
        var guild1 = new Guild("123", "server1");
        guild1.AddCustomCommand("text1", "999", "some_content_on_server1");
        guild1.AddCustomCommand("image1", "777")
            .AddAttachment("file1_on_server1", ".png", "https://example.com/file1.png");
        guild1.AddCustomCommand("textandimage1", "888", "some_more_content_on_server1")
            .AddAttachment("file2_on_server1", ".png", "https://example.com/file2.png");
        var guild2 = new Guild("456", "server2");
        guild1.AddCustomCommand("text1", "999", "some_content_on_server2");
        guild1.AddCustomCommand("image5", "666")
            .AddAttachment("file1_on_server2", ".jpg", "https://sample.com/file1.jpg");
        await _dbContext.Guilds.AddAsync(guild1);
        await _dbContext.Guilds.AddAsync(guild2);
        await _dbContext.SaveChangesAsync();
    }

    [After(Test)]
    public Task TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _httpClient.Dispose();
        return Task.CompletedTask;
    }

    [Test]
    [Arguments("text1", "some_text_content", false)]
    [Arguments("text1", null, true)]
    [Arguments("text1", null, false)]
    [Arguments("text1", "some_text_content", true)]
    [Arguments("image1", "some_text_content", false)]
    [Arguments("image1", null, true)]
    [Arguments("image1", null, false)]
    [Arguments("image1", "some_text_content", true)]
    [Arguments("imageandtext1", "some_text_content", false)]
    [Arguments("imageandtext1", null, true)]
    [Arguments("imageandtext1", null, false)]
    [Arguments("imageandtext1", "some_text_content", true)]
    [Arguments("non-existent-text1", "some_text_content", false)]
    [Arguments("non-existent-text1", null, true)]
    [Arguments("non-existent-text1", null, false)]
    [Arguments("non-existent-text1", "some_text_content", true)]
    [Arguments("non-existent-image1", "some_text_content", false)]
    [Arguments("non-existent-image1", null, true)]
    [Arguments("non-existent-image1", null, false)]
    [Arguments("non-existent-image1", "some_text_content", true)]
    [Arguments("non-existent-imageandtext1", "some_text_content", false)]
    [Arguments("non-existent-imageandtext1", null, true)]
    [Arguments("non-existent-imageandtext1", null, false)]
    [Arguments("non-existent-imageandtext1", "some_text_content", true)]
    public async Task SaveCustomCommandAsync_ExistingCommand_IsOverwritten(string commandName, string? content,
        bool sendAttachment)
    {
        ulong guildId = 123;
        ulong userId = 777;

        var attachment = sendAttachment
            ? new Attachment(new JsonAttachment { Url = $"https://example.com/{commandName}.png" })
            : null;
        if (sendAttachment)
            _handler
                .Expect(HttpMethod.Get, attachment!.Url)
                .Respond(HttpStatusCode.OK, Array.Empty<KeyValuePair<string, string>>(), "application/json",
                    new MemoryStream());

        var sut = new CustomCommandService(LoggerMock, _guildQueries, Substitute.For<IFileUploadService>(),
            DiscordSettings, _guildRepository, _httpClient);
        var result =
            await sut.SaveCustomCommandAsync(guildId.ToString(), userId.ToString(), commandName, content, attachment);

        var savedCommand = _dbContext
            .Guilds
            .Include(g => g.CustomCommands)
            .ThenInclude(cc => cc.Attachments)
            .FirstOrDefault(g => g.ExternalId == guildId.ToString())!.CustomCommands
            .FirstOrDefault(c => c.Name == commandName);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsSuccess).IsTrue();
            await Assert.That(savedCommand).IsNotNull();
            await Assert.That(savedCommand!.Name).IsEqualTo(commandName);
            await Assert.That(savedCommand.Content).IsEqualTo(content);
            await Assert.That(savedCommand.CreatorId).IsEqualTo(userId.ToString());
            if (sendAttachment)
                await Assert.That(savedCommand.Attachments.First().Url).IsEqualTo(attachment!.Url);
        }
    }

    #region GetCustomCommandAsync tests

    [Test]
    public async Task GetCustomCommandAsync_NoMatchingCommands_ReturnsErrorMsg()
    {
        ulong guildId = 123;
        var commandName = "image5";

        var sut = new CustomCommandService(LoggerMock, _guildQueries, FileUploadService, DiscordSettings,
            GuildRepositoryMock, _httpClient);

        var result = await sut.GetCustomCommandAsync(guildId.ToString(), commandName);

        await Assert.That(result.IsSuccess).IsFalse();
    }

    [Test]
    public async Task GetCustomCommandAsync_TextCommandInMatchingGuild_ReturnsMatchingTextCommand()
    {
        ulong guildId = 123;
        var commandName = "text1";

        var sut = new CustomCommandService(LoggerMock, _guildQueries, FileUploadService, DiscordSettings,
            GuildRepositoryMock, _httpClient);

        var result = await sut.GetCustomCommandAsync(guildId.ToString(), commandName);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value?.Content).IsEqualTo("some_content_on_server1");
    }

    [Test]
    public async Task GetCustomCommandAsync_ImageOnlyCommandInMatchingGuild_ReturnsImageOnly()
    {
        ulong guildId = 123;
        var commandName = "image1";
        var fileBucketName = $"{DiscordSettings.Value.BucketEnvPrefix}-discord-{guildId}";
        var fileName = "file1_on_server1";

        AmazonS3ClientMock
            .GetObjectAsync(Arg.Is<GetObjectRequest>(x => x.BucketName == fileBucketName && x.Key == fileName),
                Arg.Any<CancellationToken>())
            .Returns(new GetObjectResponse
                { ResponseStream = new MemoryStream(), Key = fileName, BucketName = fileBucketName });

        var sut = new CustomCommandService(LoggerMock, _guildQueries, FileUploadService, DiscordSettings,
            GuildRepositoryMock, _httpClient);

        var result = await sut.GetCustomCommandAsync(guildId.ToString(), commandName);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value?.Content).IsNull();
        await Assert.That(result.Value?.Attachments.All(x => x.FileName == "file1_on_server1")).IsTrue();
    }

    [Test]
    public async Task GetCustomCommandAsync_TextAndImageCommandInMatchingGuild_ReturnsBothTextAndImage()
    {
        ulong guildId = 123;
        var commandName = "textandimage1";
        var fileBucketName = $"{DiscordSettings.Value.BucketEnvPrefix}-discord-{guildId}";
        var fileName = "file2_on_server1";

        AmazonS3ClientMock
            .GetObjectAsync(Arg.Is<GetObjectRequest>(x => x.BucketName == fileBucketName && x.Key == fileName),
                Arg.Any<CancellationToken>())
            .Returns(new GetObjectResponse
                { ResponseStream = new MemoryStream(), Key = fileName, BucketName = fileBucketName });

        var sut = new CustomCommandService(LoggerMock, _guildQueries, FileUploadService, DiscordSettings,
            GuildRepositoryMock, _httpClient);

        var result = await sut.GetCustomCommandAsync(guildId.ToString(), commandName);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value?.Content).IsEqualTo("some_more_content_on_server1");
        await Assert.That(result.Value?.Attachments.All(x => x.FileName == "file2_on_server1")).IsTrue();
    }

    #endregion
}