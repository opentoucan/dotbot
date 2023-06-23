using FluentResults;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using static FluentResults.Result;

namespace Dotbot.Discord.Services;

public class FileService : IFileService
{
    private readonly IGridFSBucket _gridFsBucket;

    public FileService(IGridFSBucket gridFsBucket)
    {
        _gridFsBucket = gridFsBucket;
    }

    public async Task<Result<Stream>> GetFile(string fileName)
    {
        var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName));
        var options = new GridFSFindOptions
        {
            Limit = 1,
        };

        using var cursor = await _gridFsBucket.FindAsync(filter, options);
        var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
        return fileInfo == null ? Fail("No file found") : Ok<Stream>(await _gridFsBucket.OpenDownloadStreamAsync(fileInfo.Id));
    }

    public async Task<Result> SaveFile(string fileName, Stream fileStream)
    {
        await _gridFsBucket.UploadFromStreamAsync(fileName, fileStream);
        return Ok();
    }
}