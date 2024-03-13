using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Bot.Gateway.Dto.Responses;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;

namespace Bot.Gateway.Services;

public interface IFileUploadService
{
    Task UploadFile(string parentName, string attachmentName, Stream content);
    Task<FileDetails?> GetFile(string parentName, string filename);
}

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _amazonS3Client;
    private readonly ILogger<FileUploadService> _logger;
    public FileUploadService(IAmazonS3 amazonS3Client, ILogger<FileUploadService> logger)
    {
        _amazonS3Client = amazonS3Client;
        _logger = logger;
    }

    public async Task UploadFile(string parentName, string attachmentName, Stream content)
    {
        try
        {
            _logger.LogInformation("Saving file {attachment} into bucket {bucket}", attachmentName, parentName);
            using var fileTransferUtility = new TransferUtility(_amazonS3Client);
            var bucketsResponse = await _amazonS3Client.ListBucketsAsync();
            if(bucketsResponse.Buckets.FirstOrDefault(bucket => bucket.BucketName == parentName) == null)
                await _amazonS3Client.PutBucketAsync(parentName);
            
            var transferRequest = new TransferUtilityUploadRequest
            {
                BucketName = parentName,
                InputStream = content,
                ContentType = MimeTypes.GetMimeType(attachmentName),
                Key = attachmentName,
                DisablePayloadSigning = _amazonS3Client.Config.ServiceURL.StartsWith("https")
            };
            await fileTransferUtility.UploadAsync(transferRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment into bucket");
            throw;
        }
    }
    
    public async Task<FileDetails?> GetFile(string parentName, string filename)
    {
        try
        {
            var response = await _amazonS3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = parentName,
                Key = filename
            });

            return new FileDetails(response.ResponseStream, response.Key, response.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve image");
        }

        return null;
    }
}